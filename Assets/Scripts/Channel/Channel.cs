using System;
using System.Collections.Generic;
using UnityEngine;

public class Channel : MonoBehaviour
{
    public static event Action<int, Mood, float> OnChannelUpdate;

    [SerializeField] List<ChannelDisplay> drawers = new List<ChannelDisplay> ();

    int currentSlot = 0;
    int slotCount = 3;

    private void Awake()
    {
        AbsorbState.OnMarbleAbsorbtion += OnMarbleAbsorbed;
    }

    private void OnDestroy()
    {
        AbsorbState.OnMarbleAbsorbtion -= OnMarbleAbsorbed;
    }

    private void OnMarbleAbsorbed(Mood marbleMood, float intensity)
    {
        OnChannelUpdate?.Invoke(currentSlot, marbleMood, intensity);
        drawers[currentSlot].SetDisplay(marbleMood, intensity);
        currentSlot = (currentSlot + 1) % slotCount;
    }
}