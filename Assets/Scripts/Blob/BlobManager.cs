using System;
using System.Collections.Generic;
using UnityEngine;

public class BlobManager : MonoBehaviour
{
    [SerializeField] Material blobMaterial;
    [SerializeField]
    MoodInput first;
    [SerializeField]
    MoodInput second;
    [SerializeField]
    MoodInput third;

    [SerializeField]
    MoodProperties angerProp;
    [SerializeField]
    MoodProperties joiceProp;
    [SerializeField]
    MoodProperties sadnessProp;
    [SerializeField]
    MoodProperties fearProp;

    [SerializeField] List<CircleData> circles = new List<CircleData>();

    [SerializeField] Color blobEdgeColor;
    [SerializeField] Color blobInnerColor;

    [SerializeField]
    [Range(0, 3)]
    int innerRenderMethod;

    [SerializeField]
    [Range(0, 9)]
    int outerRenderMethod;

    [Header("Beat Parameters ")]
    [SerializeField]
    float BPM;
    float beatSpeed;
    [SerializeField]
    AnimationCurve beatCurve;
    float beatFactor;

    [Header("Scale Parameters")]
    [SerializeField] float scaleFactorOnBeat;

    private void Update()
    {
        blobMaterial.SetFloat("_UnityTime", Time.time);

        blobInnerColor = GetBlendColor();
        blobEdgeColor = blobInnerColor;
        Color.RGBToHSV(blobInnerColor, out float h, out float s, out float v);
        blobEdgeColor = Color.HSVToRGB(h, 1, v);
        blobMaterial.SetColor("_InnerColor", blobInnerColor);
        blobMaterial.SetColor("_EdgeColor", blobEdgeColor);

        beatSpeed = BPM / 60;
        beatFactor = beatCurve.Evaluate((Time.time * beatSpeed) % 1);
        blobMaterial.SetFloat("_LightFactor", beatFactor);

        int circleCount = circles.Count;
        blobMaterial.SetInt("_CircleCount", circleCount);
        blobMaterial.SetInt("_innerRenderMethod", innerRenderMethod);
        blobMaterial.SetInt("_outerRenderMethod", outerRenderMethod);

        Vector4[] rola = new Vector4[circleCount];
        float[] rotationSpeed = new float[circleCount];

        for (int i = 0; i < circleCount; i++)
        {
            circles[i].lerpPhase += (circles[i].lerpSpeed ) * Time.deltaTime;

            rola[i] = new Vector4(
                circles[i].radius + scaleFactorOnBeat * (beatFactor * circles[i].radius),
                circles[i].orbitRadius,
                circles[i].lerpPhase,
                circles[i].angleOffset);
            rotationSpeed[i] = circles[i].rotationSpeed;
        }

        //Awfull af way to pass data 
        blobMaterial.SetVectorArray("_Circles", rola);
        blobMaterial.SetFloatArray("_RotationSpeeds", rotationSpeed);
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
public class CircleData
{
    public float radius;
    public float orbitRadius;
    public float lerpSpeed;
    public float angleOffset;
    public float rotationSpeed;
    [HideInInspector]public float lerpPhase;
}