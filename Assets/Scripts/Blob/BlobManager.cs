using System;
using System.Collections.Generic;
using UnityEngine;

public class BlobManager : MonoBehaviour
{
    public static BlobManager instance;

    [Header("Channel inputs ")]
    [SerializeField] Material blobMaterial;

    [SerializeField]
    MoodInput[] emotions = new MoodInput[3];

    [Header("Movements")]
    [SerializeField] List<Part> partsData = new List<Part>();
    [SerializeField] AnimationCurve speedCurve;
    [SerializeField] float movementAreaRadius;
    [SerializeField] float dispertionFactor;
    [SerializeField] float dispertionMax;
    [SerializeField] float speedFactor;
    [SerializeField][Range(0f, 1f)] float movementType;
    [SerializeField] AnimationCurve speedFactorCurve;
    [SerializeField] float lowSpeedValue;
    [SerializeField] float highSpeedValue;
    [SerializeField] AnimationCurve shakeCurve;
    [SerializeField] float maxShakeRange;
    [SerializeField, Range(0, 1)] float shakeFactor;

    [Header("Render")]
    [SerializeField][Range(0f, 10)] float auraFrequency;
    [SerializeField][Range(-100f, 100f)] float auraSpeed;
    [SerializeField][Range(0f, 100f)] float auraRange;
    [SerializeField][Range(-1f, 5f)] float auraWidth;
    [SerializeField][Range(0f, 10f)] float uvLengthFactor;
    [SerializeField][Range(0f, 100f)] float lightSdScale;
    [SerializeField][Range(-10f, 10f)] float xOffset;
    [SerializeField][Range(-10f, 10f)] float yOffset;
    float auraOffset;

    [Header("RTPC dependent parameters  ")]
    [SerializeField] RtpcDependent lightFactor;
    [SerializeField] RtpcDependent scaleFactorRtpc;
    [SerializeField] RtpcDependent auraRangeRTPC; 

    [Header("Debug")]
    [SerializeField] Color blobEdgeColor;
    [SerializeField] Color blobInnerColor;
    [SerializeField]
    [Range(0, 4)]
    int innerRenderMethod;
    [SerializeField]
    [Range(0, 10)]
    int outerRenderMethod;
    [SerializeField][Range(0, 100)] int rtpcValue;
    [SerializeField] AK.Wwise.RTPC rTPC;
    [SerializeField] AnimationCurve waveFormCurve;
    [SerializeField] AK.Wwise.RTPC beat;

    Vector4[] toShader;
    int circleCount;

    State previousState = new();
    State computedState = new();
    float stateLerp;
    bool stateLerping = false;

    MarbleAuraManager marbleAura = new();

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
        //This includes the four marble circles
        toShader = new Vector4[circleCount + 4];

        for (int i = 0; i < circleCount; i++)
        {
            partsData[i].origin = Vector2.zero;
            partsData[i].destination = new Vector2(
                UnityEngine.Random.Range(-movementAreaRadius, movementAreaRadius),
                UnityEngine.Random.Range(-movementAreaRadius, movementAreaRadius));
            partsData[i].lerpPhase = 0;
        }

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

        blobMaterial.SetFloat("_auraRange", auraRange * auraRangeRTPC.Get());
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

        //Compute shakeFactor
        Vector2 shakeOffset = new Vector2(UnityEngine.Random.Range(-1,1), UnityEngine.Random.Range(-1, 1)).normalized * maxShakeRange;
        shakeOffset *= shakeFactor;

        for (int i = 0; i < circleCount; i++)
        {
            //Linear lerp constant speed
            float newSpeedFactor = lowSpeedValue + (speedFactorCurve.Evaluate(speedFactor) * highSpeedValue);
            partsData[i].lerpPhase += Time.deltaTime * (partsData[i].lerpSpeed / (Vector2.Distance(partsData[i].origin, partsData[i].destination) * 2)) * newSpeedFactor;

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
                float movementAreaRad = movementAreaRadius + dispertionFactor * dispertionMax;
                partsData[i].destination.Set(UnityEngine.Random.Range(-movementAreaRad, movementAreaRad), UnityEngine.Random.Range(-movementAreaRad, movementAreaRad));
                partsData[i].lerpPhase = 0;
            }

            partsData[i].currentPos = computePos + shakeOffset;

            toShader[i] = new Vector4(
                partsData[i].currentPos.x,
                partsData[i].currentPos.y,
                partsData[i].radius + (scaleFactorRtpc.Get() * partsData[i].radius),
                0);
        }

        int partCount = circleCount;
        Vector4[] extraColors = new Vector4[4];


        marbleAura.ProcessMarblesAura();
        //Add the marble extra aura 
        foreach (MarbleAuraRenderState state in marbleAura.marbles2Render)
        {
            partCount++;
            Vector2 marbleUvPos = Utils.World2UV(state.marble.trans.position);
            float radius = Utils.World2UV(state.marble.trans.localScale).x / 1.9f * state.scale;
            
            extraColors[partCount - circleCount] = state.marble.mat.color;

            toShader[partCount-1] = new Vector4(marbleUvPos.x, marbleUvPos.y, radius);
        }

        blobMaterial.SetInt("_CircleCount", partCount);
        blobMaterial.SetVectorArray("_Circles", toShader);
        blobMaterial.SetVectorArray("_CirclesColors", extraColors);
    }

    #region State
    private State MakeSnapShot()
    {
        return new State { color = blobInnerColor, speed = speedFactor, shake = shakeFactor, dispertion = movementAreaRadius};
    }
    private void LerpToComputedState(float t)
    {
        blobInnerColor = Color.Lerp(previousState.color, computedState.color, t);
        Color.RGBToHSV(blobInnerColor, out float h, out float s, out float v);
        blobEdgeColor = Color.HSVToRGB(h, s, 1);

        speedFactor = Mathf.Lerp(previousState.speed, computedState.speed, t);
        shakeFactor = Mathf.Lerp(previousState.shake, computedState.shake, t);
        dispertionFactor = Mathf.Lerp(previousState.dispertion, computedState.dispertion, t);
    }
    public void StartLerping()
    {
        stateLerp = 0;
        stateLerping = true;
        previousState = MakeSnapShot();
        computedState = new State 
        { 
            color = GetBlendColor(), 
            speed = ProcessSpeed(), 
            shake = ProcessShakeFactor(),
            dispertion = ProcessDispertion()
        };
    }
    public void SetMoodState(Mood mood, float intensity, int index)
    {
        emotions[index].mood = mood;
        emotions[index].intensity = intensity;
    }
    #endregion
    #region Mood Dependent param
    private float ProcessShakeFactor()
    {
        float angerCount = 0;
        for (int i = 0; i < emotions.Length; i++)
        {
            if (emotions[i].mood == Mood.Anger && emotions[i].intensity > 0)
                angerCount++;
        }
        return (angerCount / emotions.Length);
    }
    private float ProcessSpeed()
    {
        //Speed only depends on the intensity of the first slot 
        return emotions[0].intensity / 100;
    }
    private float ProcessDispertion()
    {
        float fearCount = 0;
        for (int i = 0; i < emotions.Length; i++)
        {
            if (emotions[i].mood == Mood.Fear && emotions[i].intensity > 0)
                fearCount++;
        }
        return (fearCount / emotions.Length);
    }
    #endregion 
    #region Color
    private Color GetBlendColor()
    {
        float divisionValue = 0;
        for (int i = 0; i < emotions.Length; i++)
        {
            divisionValue += emotions[i].intensity;
        }

        //This mean absolutly no input 
        if (divisionValue == 0)
        {
            return Color.gray;
        }

        float r = 0, g = 0, b = 0;
        for (int i = 0; i < emotions.Length; i++)
        {
            Color currentColor = GetMoodColor(emotions[i].mood, emotions[i].intensity);
            r += currentColor.r * emotions[i].intensity;
            g += currentColor.g * emotions[i].intensity;
            b += currentColor.b * emotions[i].intensity;
        }

        return new Color(r, g, b)/divisionValue;
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
    public Vector2 GetClosestPartPos(Vector2 UvPos)
    {
        return GetClosestPartRef(UvPos).currentPos;
    }
    public Part GetClosestPartRef(Vector2 UvPos)
    {
        Part closestPartPos = new();
        float sd = 2;

        float currentSD;
        foreach (Part part in partsData)
        {
            currentSD = (part.currentPos - UvPos).magnitude;
            if (currentSD < sd)
            {
                sd = currentSD;
                closestPartPos = part;
            }
        }

        return closestPartPos;
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
    public float shake;
    public float dispertion;

    public State()
    {
        color = Color.gray;
        speed = 0.05f;
        shake = 0;
        dispertion = 0.2f;
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
                    return outputMin + rangeCurve.Evaluate(normalizedRtpc) * (outputMax - outputMin);
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
