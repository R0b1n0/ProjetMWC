using System;
using UnityEngine;

[CreateAssetMenu(menuName = "EmotionValues", fileName = "EmotionValues")]
public class EmotionParameters : ScriptableObject
{
    [SerializeField] public MoodProperties Anger;
    [SerializeField] public MoodProperties Joice;
    [SerializeField] public MoodProperties Sadness;
    [SerializeField] public MoodProperties Fear;

    static EmotionParameters instance;
    public static EmotionParameters Instance
    {
        get
        {
            if (!instance)
            {
                instance = Resources.Load<EmotionParameters>("Moods");
            }
            return instance;
        }
    }

    public MoodProperties GetMoodInfo(Mood mood)
    {
        switch (mood)
        {
            case Mood.Anger:
                return Anger;
            case Mood.Joice:
                return Joice;
            case Mood.Sadness:
                return Sadness;
            case Mood.Fear:
                return Fear;
        }
        return new MoodProperties();
    }

    public Color GetMoodColor(Mood mood, float intensity)
    {
        MoodProperties palet = GetMoodInfo(mood);

        if (intensity < 1f / 3f)
            return palet.marbleColor;
        else if (intensity < 2f / 3f)
            return palet.marbleColor1;
        else return palet.marbleColor2;
    }
}


public enum Mood
{
    Anger,
    Joice,
    Fear,
    Sadness
}

[Serializable]
public struct MoodProperties
{
    public Color minColor;
    public Color maxColor;
    public Color marbleColor;
    public Color marbleColor1;
    public Color marbleColor2;
}