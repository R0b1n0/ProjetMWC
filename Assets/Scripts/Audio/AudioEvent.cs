using System;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public class AudioEvent : MonoBehaviour
{
    [Header("Events References")]
    [SerializeField] AK.Wwise.Event playAll;
    [SerializeField] AK.Wwise.Switch slot1;

    [SerializeField, Range(0, 2)] int slot;
    [Header("Params")]
    [SerializeField] AudioParam parameters;


    private void Awake()
    {
        AbsorbState.OnMarbleAbsorbtion += OnMarbleAbsorbed;
    }

    private void OnDestroy()
    {
        AbsorbState.OnMarbleAbsorbtion -= OnMarbleAbsorbed;
    }

    private void Start()
    {
        playAll.Post(gameObject);
    }

    private void OnMarbleAbsorbed(Mood marbleMood, float intensity)
    {
        Debug.Log($"Slot {slot} was set to {marbleMood}, {intensity}");
        parameters.slotsInfo[slot].slotSwitchEvents[marbleMood].SetValue(gameObject);
        parameters.slotsInfo[slot].intensity.SetGlobalValue(intensity * 100);
    }
}

[Serializable]
public class AudioParam
{
    public SlotParam[] slotsInfo = new SlotParam[3];
}

[Serializable]
public class SlotParam
{
    [SerializedDictionary("Marble Mood", "SwitchToSet")]
    public SerializedDictionary<Mood, AK.Wwise.Switch> slotSwitchEvents;
    public AK.Wwise.RTPC intensity;
}

/* 
 Todo 
struct {
Dictionnary[Mood,Switch] data [3] 
 }
*/
