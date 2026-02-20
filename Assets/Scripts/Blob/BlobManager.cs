using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class BlobManager : MonoBehaviour
{
    public static BlobManager instance;

    [Header("Channel inputs ")]
    [SerializeField] Material blobMaterial;
    [SerializeField]
    MoodInput first;
    [SerializeField]
    MoodInput second;
    [SerializeField]
    MoodInput third;

    [Header("Movements")]
    [SerializeField] List<Part> partsData = new List<Part>();
    [SerializeField] AnimationCurve speedCurve;
    [SerializeField] float movementAreaRadius;
    [SerializeField] float speedFactor;
    [SerializeField][Range(0f, 1f)] float movementType;

    [Header("Render")]
    [SerializeField][Range(0f, 10)] float auraFrequency;
    [SerializeField][Range(-100f, 100f)] float auraSpeed;
    [SerializeField][Range(0f, 100f)] float auraRange;
    [SerializeField][Range(-1f, 5f)] float auraWidth;
    [SerializeField][Range(0f, 10f)] float uvLengthFactor;
    [SerializeField][Range(0f, 100f)] float lightSdScale;
    [SerializeField][Range(-10f, 10f)] float xOffset;
    [SerializeField][Range(-10f, 10f)] float yOffset;
    [SerializeField] RtpcDependent lightFactor;
    float auraOffset;

    [Header("Beat Parameters ")]
    float beatFactor;
    [SerializeField] float scaleFactorOnBeat;

    MarbleAuraManager marbleAura = new();


    [Header("Debug")]
    [SerializeField] Color blobEdgeColor;
    [SerializeField] Color blobInnerColor;
    [SerializeField]
    [Range(0, 3)]
    int innerRenderMethod;
    [SerializeField]
    [Range(0, 9)]
    int outerRenderMethod;
    [SerializeField][Range(0, 100)] int rtpcValue;
    [SerializeField] AK.Wwise.RTPC rTPC;
    [SerializeField] AnimationCurve waveFormCurve;
    [SerializeField] AK.Wwise.RTPC beat;
    [SerializeField] AK.Wwise.RTPC auraRangeRTPC;

    Vector4[] toShader;
    int circleCount;

    State previousState = new();
    State computedState = new();
    float stateLerp;
    bool stateLerping = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        circleCount = partsData.Count;
        blobMaterial.SetInt("_CircleCount", circleCount);
        toShader = new Vector4[circleCount + 4];

        for (int i = 0; i < circleCount; i++)
        {
            partsData[i].origin = Vector2.zero;
            partsData[i].destination = new Vector2(
                UnityEngine.Random.Range(-movementAreaRadius, movementAreaRadius),
                UnityEngine.Random.Range(-movementAreaRadius, movementAreaRadius));
            partsData[i].lerpPhase = 0;
        }

        //speedFactor = computedState.speed;
        LerpToComputedState(1);
    }
    private void Update()
    {
        //Set pos
        UpdatePartsPos();

        blobMaterial.SetFloat("_UnityTime", Time.time);

        auraOffset += Time.deltaTime * auraSpeed;

        blobMaterial.SetColor("_InnerColor", blobInnerColor);
        blobMaterial.SetColor("_EdgeColor", blobEdgeColor);
        blobMaterial.SetFloat("_auraF", auraFrequency);

        blobMaterial.SetFloat("_auraRange", auraRange * ( 1 + ((auraRangeRTPC.GetGlobalValue() + 15 ) / 8)));
        blobMaterial.SetFloat("_auraOffset", auraOffset);
        blobMaterial.SetFloat("_auraWidth", auraWidth);
        blobMaterial.SetFloat("_uvLengthFactor", uvLengthFactor);
        blobMaterial.SetFloat("_xOffset", xOffset);
        blobMaterial.SetFloat("_yOffset", yOffset);
        blobMaterial.SetFloat("_lightSdScale", lightSdScale);

        blobMaterial.SetFloat("_LightFactor", lightFactor.Get());

        blobMaterial.SetInt("_innerRenderMethod", innerRenderMethod);
        blobMaterial.SetInt("_outerRenderMethod", outerRenderMethod);

        if (stateLerping)
        {
            if (stateLerp >= 1)
            {
                stateLerping = false;
                LerpToComputedState(1);
            }
            else
            {
                LerpToComputedState(stateLerp);
                stateLerp += Time.deltaTime;
            }
        }

        rTPC.SetGlobalValue(rtpcValue);
    }
    private void UpdatePartsPos()
    {
        Vector2 computePos;
        for (int i = 0; i < circleCount; i++)
        {
            //Linear lerp constant speed
            partsData[i].lerpPhase += Time.deltaTime * (partsData[i].lerpSpeed / (Vector2.Distance(partsData[i].origin, partsData[i].destination) * 2)) * speedFactor;

            float lerpValue = speedCurve.Evaluate(partsData[i].lerpPhase);
            //Linear move 
            Vector2 linearLerp = Vector2.Lerp(partsData[i].origin, partsData[i].destination, lerpValue);

            //Curved movement 
            Vector2 circularLerp = Vector3.Slerp(partsData[i].origin, partsData[i].destination, lerpValue);

            computePos = Vector2.Lerp(linearLerp, circularLerp, movementType);

            if (partsData[i].lerpPhase >= 1)
            {
                //Circle finished lerping
                partsData[i].origin = partsData[i].destination;
                partsData[i].destination.Set(UnityEngine.Random.Range(-movementAreaRadius, movementAreaRadius), UnityEngine.Random.Range(-movementAreaRadius, movementAreaRadius));
                partsData[i].lerpPhase = 0;
            }

            partsData[i].currentPos = computePos;

            toShader[i] = new Vector4(
                computePos.x,
                computePos.y,
                partsData[i].radius + scaleFactorOnBeat * (beatFactor * partsData[i].radius),
                0);
        }

        int partCount = circleCount;
        //Add the marble extra aura 
        marbleAura.ProcessMarblesAura();
        foreach(MarbleAuraRenderState state in marbleAura.marbles2Render)
        {
            partCount++;
            Vector2 marbleUvPos = Utils.World2UV(state.marble.trans.position);
            float radius = Utils.World2UV(state.marble.trans.localScale).x / 1.9f * state.scale;

            toShader[partCount-1] = new Vector4(marbleUvPos.x, marbleUvPos.y, radius);
        }

        blobMaterial.SetInt("_CircleCount", partCount);
        blobMaterial.SetVectorArray("_Circles", toShader);
    }

    #region State
    private float ComputeSpeed()
    {
        MoodProperties firstData = GetMoodData(first.mood);
        MoodProperties secondData = GetMoodData(second.mood);
        MoodProperties thirdData = GetMoodData(third.mood);
        float divisionValue = (first.intensity + second.intensity + third.intensity);

        return divisionValue != 0 ?
            (firstData.speedFactor * Mathf.Pow(first.intensity, 2) +
            secondData.speedFactor * Mathf.Pow(second.intensity, 2) +
            thirdData.speedFactor * Mathf.Pow(third.intensity, 2)) / 100 / divisionValue
            : 0.05f;
    }
    private State MakeSnapShot()
    {
        return new State { color = blobInnerColor, speed = speedFactor };
    }
    private void LerpToComputedState(float t)
    {
        //Colors
        blobInnerColor = Color.Lerp(previousState.color, computedState.color, t);
        Color.RGBToHSV(blobInnerColor, out float h, out float s, out float v);
        blobEdgeColor = Color.HSVToRGB(h, s, 1);

        speedFactor = Mathf.Lerp(previousState.speed, computedState.speed, t);
    }
    public void StartLerping()
    {
        stateLerp = 0;
        stateLerping = true;
        previousState = MakeSnapShot();
        computedState = new State { color = GetBlendColor(), speed = ComputeSpeed() };
    }
    #endregion
    #region Color
    private Color GetBlendColor()
    {
        float divisionValue = (first.intensity + second.intensity + third.intensity);

        //This mean absolutly no input 
        if (divisionValue == 0)
        {
            return Color.gray;
        }

        Color color1 = GetMoodColor(first.mood, first.intensity);
        Color color2 = GetMoodColor(second.mood, second.intensity);
        Color color3 = GetMoodColor(third.mood, third.intensity);


        float r = (color1.r * first.intensity + color2.r * second.intensity + color3.r * third.intensity) / divisionValue;
        float g = (color1.g * first.intensity + color2.g * second.intensity + color3.g * third.intensity) / divisionValue;
        float b = (color1.b * first.intensity + color2.b * second.intensity + color3.b * third.intensity) / divisionValue;

        return new Color(r, g, b);
    }
    private Color GetMoodColor(Mood mood, float intensity)
    {
        EmotionParameters inst = EmotionParameters.Instance;
        switch (mood)
        {
            case Mood.Anger:
                return Color.Lerp(inst.Anger.minColor, inst.Anger.maxColor, intensity / 100f);
            case Mood.Joice:
                return Color.Lerp(inst.Joice.minColor, inst.Joice.maxColor, intensity / 100f);
            case Mood.Fear:
                return Color.Lerp(inst.Fear.minColor, inst.Fear.maxColor, intensity / 100f);
            case Mood.Sadness:
                return Color.Lerp(inst.Sadness.minColor, inst.Sadness.maxColor, intensity / 100f);
        }
        return Color.white;
    }
    private MoodProperties GetMoodData(Mood mood)
    {
        switch (mood)
        {
            case Mood.Anger:
                return EmotionParameters.Instance.Anger;
            case Mood.Joice:
                return EmotionParameters.Instance.Joice;
            case Mood.Fear:
                return EmotionParameters.Instance.Fear;
            case Mood.Sadness:
                return EmotionParameters.Instance.Sadness;
        }
        return EmotionParameters.Instance.Anger;
    }
    #endregion
    #region Utils
    public bool IsWithinBlobBounds(Vector2 UvPos, float uvRadius)
    {
        bool inBounds = false;

        for(int i = 0; i < circleCount; i++)
        {
            if ((partsData[i].currentPos - UvPos).magnitude - uvRadius - partsData[i].radius < 0.07)
            {
                inBounds = true;
                break;
            }
        }

        return inBounds;
    }
    #endregion
}


[Serializable]
public struct MoodInput
{
    [Range(0f, 100f)]
    public float intensity;
    public Mood mood;
}

public class State
{
    public Color color;
    public float speed;
    public State()
    {
        color = Color.gray;
        speed = 0.05f;
    }
}

[Serializable]
public class Part
{
    public float radius;
    public float lerpSpeed;
    //All of those are UV value
    [HideInInspector] public Vector2 currentPos;
    [HideInInspector] public Vector2 destination;
    [HideInInspector] public Vector2 origin;
    [HideInInspector] public float lerpPhase;
}

[Serializable]
public struct RtpcDependent
{
    public AK.Wwise.RTPC rtpc;
    public EEvaluationMode evaluationMode;

    public int rtpcMin;
    public int rtpcMax;
    public AnimationCurve normalizedCurve;
    public AnimationCurve rangeCurve;
    public float baseValue;

    public float outputMin;
    public float outputMax;

    public float Get()
    {
        if (rtpc.WwiseObjectReference == null) 
            return baseValue;

        float normalizedRtpc = (rtpc.GetGlobalValue() - rtpcMin) / Math.Abs(rtpcMax - rtpcMin);

        if (normalizedRtpc == float.NaN)
            return 0;

        switch (evaluationMode)
        {
            case EEvaluationMode.LinearNormalized:
                return normalizedRtpc;

            case EEvaluationMode.LinearRange:
                    return outputMin + normalizedRtpc * (outputMax - outputMin);
                
            case EEvaluationMode.NormalizedCurve:
                return normalizedCurve.Evaluate(normalizedRtpc);
                
            case EEvaluationMode.CurvedRange:
                    return outputMin + normalizedCurve.Evaluate(normalizedRtpc) * (outputMax - outputMin);
        }

        return baseValue;
    }

    public enum EEvaluationMode
    {
        LinearNormalized,
        LinearRange,
        NormalizedCurve,
        CurvedRange
    }
}
