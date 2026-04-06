using System;
using System.Collections.Generic;
using UnityEngine;

public class Channel : MonoBehaviour
{
    public static event Action<int, Mood, float> OnChannelUpdate;
    public static event Action<int> OnChannelClear;

    [SerializeField] List<ChannelDisplay> drawers = new List<ChannelDisplay>();

    public int currentSlot { get; private set; }
    int slotCount = 3;

    private void Awake()
    {
        for (int i = 0; i < drawers.Count; i++) drawers[i].SetId(i);
        AbsorbState.OnMarbleAbsorbtion += OnMarbleAbsorbed;
    }
    private void Start()
    {
        SetCurrentChannel(0, true);
    }
    private void OnDestroy()
    {
        AbsorbState.OnMarbleAbsorbtion -= OnMarbleAbsorbed;
    }

    private void OnMarbleAbsorbed(Mood marbleMood, float intensity)
    {
        OnChannelUpdate?.Invoke(currentSlot, marbleMood, intensity);
        drawers[currentSlot].SetDisplay(marbleMood, intensity);
        SetCurrentChannel((currentSlot + 1) % slotCount);
    }
    public bool TrySetCurrentChannel(int channelId)
    {
        if (channelId != currentSlot)
        {
            SetCurrentChannel(channelId);
            return true;
        }

        return false;
    }
    public void EmptyCurrentChannel()
    {
        drawers[currentSlot].Clear();
        OnChannelClear?.Invoke(currentSlot);
    }
    public void SetCurrentChannel(int id, bool force = false)
    {
        if (id == currentSlot && !force)
            return;

        drawers[currentSlot].UnSelect();
        currentSlot = id;
        drawers[currentSlot].Select();
    }
}