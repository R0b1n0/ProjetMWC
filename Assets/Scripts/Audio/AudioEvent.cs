using AYellowpaper.SerializedCollections;
using System;
using UnityEngine;

public class AudioEvent : MonoBehaviour
{
    [Header("Events References")]
    [SerializeField] AK.Wwise.Event playAll;

    [Header("Params")]
    [SerializeField] AudioParam parameters;


    private void Awake()
    {
        Channel.OnChannelUpdate += OnMarbleAbsorbed;
        Channel.OnChannelClear += OnChannelClear;
    }

    private void OnDestroy()
    {
        Channel.OnChannelUpdate -= OnMarbleAbsorbed;
    }

    private void Start()
    {
        playAll.Post(gameObject);
    }
    private void OnChannelClear(int slot)
    {
        parameters.slotsInfo[slot].mutEvent.Post(gameObject);
    }
    private void OnMarbleAbsorbed(int slot, Mood marbleMood, float intensity)
    {
        parameters.slotsInfo[slot].slotSwitchEvents[marbleMood].SetValue(gameObject);
        parameters.slotsInfo[slot].intensity.SetGlobalValue(intensity * 100);
        parameters.slotsInfo[slot].unmutEvent.Post(gameObject);
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
    public AK.Wwise.Event unmutEvent;
    public AK.Wwise.Event mutEvent;
}

