using System;
using System.Collections.Generic;
using UnityEngine;

public class BlobManager : MonoBehaviour
{
    [Header("Channel inputs ")]
    [SerializeField] Material blobMaterial;
    [SerializeField]
    MoodInput first;
    [SerializeField]
    MoodInput second;
    [SerializeField]
    MoodInput third;

    [Header("Emotions parameters ")]
    [SerializeField]
    MoodProperties angerProp;
    [SerializeField]
    MoodProperties joiceProp;
    [SerializeField]
    MoodProperties sadnessProp;
    [SerializeField]
    MoodProperties fearProp;

    [Header("Movements")]
    [SerializeField] List<Part> partsData = new List<Part>();
    [SerializeField] AnimationCurve speedCurve;
    [SerializeField] float movementAreaRadius;
    [SerializeField] float speedFactor;
    [SerializeField][Range(0f, 1f)] float movementType;

    [Header("Render")]
    [SerializeField][Range(0f,10)] float auraFrequency;
    [SerializeField][Range(0f, 100f)] float auraRange;
    [SerializeField][Range(-1f, 5f)] float auraWidth;
    [SerializeField][Range(0f, 10f)] float uvLengthFactor;

    [Header("Beat Parameters ")]
    [SerializeField]
    float BPM;
    float beatSpeed;
    [SerializeField]
    AnimationCurve beatCurve;
    float beatFactor;
    [SerializeField] float scaleFactorOnBeat;
    

    [Header("Debug")]
    [SerializeField] Color blobEdgeColor;
    [SerializeField] Color blobInnerColor;
    [SerializeField]
    [Range(0, 3)]
    int innerRenderMethod;
    [SerializeField]
    [Range(0, 9)]
    int outerRenderMethod;

    Vector4[] toShader;
    int circleCount;

    private void Awake()
    {
        circleCount = partsData.Count;
        blobMaterial.SetInt("_CircleCount", circleCount);
        toShader = new Vector4[circleCount];

        for (int i = 0; i < circleCount; i++)
        {
            partsData[i].destination = partsData[i].origin = Vector2.zero;
        }
    }

    private void Update()
    {
        blobMaterial.SetFloat("_UnityTime", Time.time);

        blobInnerColor = GetBlendColor();
        blobEdgeColor = blobInnerColor;
        Color.RGBToHSV(blobInnerColor, out float h, out float s, out float v);
        blobEdgeColor = Color.HSVToRGB(h, 1, v);
        blobMaterial.SetColor("_InnerColor", blobInnerColor);
        blobMaterial.SetColor("_EdgeColor", blobEdgeColor);
        blobMaterial.SetFloat("_auraF", auraFrequency);
        blobMaterial.SetFloat("_auraRange", auraRange);
        blobMaterial.SetFloat("_auraWidth", auraWidth);
        blobMaterial.SetFloat("_uvLengthFactor", uvLengthFactor);

        beatSpeed = BPM / 60;
        beatFactor = beatCurve.Evaluate((Time.time * beatSpeed) % 1);
        blobMaterial.SetFloat("_LightFactor", beatFactor);

        
        
        blobMaterial.SetInt("_innerRenderMethod", innerRenderMethod);
        blobMaterial.SetInt("_outerRenderMethod", outerRenderMethod);

        //Set pos
        UpdatePartsPos();
    }

    private void UpdatePartsPos()
    {
        Vector2 computePos;
        for (int i = 0; i < circleCount; i++)
        {
            //Linear lerp constant speed
            partsData[i].lerpPhase += Time.deltaTime * (partsData[i].lerpSpeed / (Vector2.Distance(partsData[i].origin, partsData[i].destination) *2) )  * speedFactor;


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

            toShader[i] = new Vector4(
                computePos.x,
                computePos.y,
                partsData[i].radius + scaleFactorOnBeat * (beatFactor * partsData[i].radius),
                0);
        }
        blobMaterial.SetVectorArray("_Circles", toShader);
    }

    private Color GetBlendColor()
    {
        Color color1 = GetMoodColor(first.mood, first.intensity);
        Color color2 = GetMoodColor(second.mood, second.intensity);
        Color color3 = GetMoodColor(third.mood, third.intensity);

        float divisionValue = (first.intensity + second.intensity + third.intensity);

        float r = (color1.r * first.intensity + color2.r * second.intensity + color3.r * third.intensity) / divisionValue;
        float g = (color1.g * first.intensity + color2.g * second.intensity + color3.g * third.intensity) / divisionValue;
        float b = (color1.b * first.intensity + color2.b * second.intensity + color3.b * third.intensity) / divisionValue;

        return new Color(r, g, b );

    }

    private Color GetMoodColor(Mood mood, float intensity)
    {
        switch (mood)
        {
            case Mood.Anger:
                return Color.Lerp(angerProp.minColor,angerProp.maxColor, intensity / 100f );
            case Mood.Joice:
                return Color.Lerp(joiceProp.minColor, joiceProp.maxColor, intensity / 100f);
            case Mood.Fear:
                return Color.Lerp(fearProp.minColor, fearProp.maxColor, intensity / 100f);
            case Mood.Sadness:
                return Color.Lerp(sadnessProp.minColor, sadnessProp.maxColor, intensity / 100f);
        }
        return Color.white;
    }

}


[Serializable]
public struct MoodInput
{
    [Range(1f, 100f)] 
    public float intensity;
    public Mood mood;
}

[Serializable]
public struct MoodProperties
{
    public Color minColor;
    public Color maxColor;
}

public enum Mood
{
    Anger, 
    Joice,
    Fear,
    Sadness
}


[Serializable]
public class Part
{
    public float radius;
    public float lerpSpeed;
    [HideInInspector]public Vector2 destination;
    [HideInInspector]public Vector2 origin;
    [HideInInspector]public float lerpPhase;
}