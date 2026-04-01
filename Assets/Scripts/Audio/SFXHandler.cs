using System;
using UnityEngine;

public class SFXHandler : MonoBehaviour
{
    [SerializeField] AK.Wwise.Event OnMarbleSelect; 
    [SerializeField] AK.Wwise.Event OnMarbleRelease; 
    [SerializeField] AK.Wwise.Event OnLevelTwo; 
    [SerializeField] AK.Wwise.Event OnLevelThree; 
    [SerializeField] AK.Wwise.Event OnAbsrobtionBegin; 
    [SerializeField] AK.Wwise.Event OnMorphBegin;

    private void Awake()
    {
        DraggingState.OnMarbleDragBegin += OnMarbleGrabbed;
        DraggingState.OnMarbleDragEnd += OnMarbleReleased;
        DraggingState.OnMarbleLevelUpdate += OnMarbleLevelUpdate;
        AbsorbState.OnAbsorbtionBegin += OnMarbleAbsorbionBegin;
        Channel.OnChannelUpdate += OnMarbleAbsorbed;
    }

    private void OnDestroy()
    {
        DraggingState.OnMarbleDragBegin -= OnMarbleGrabbed;
        DraggingState.OnMarbleDragEnd -= OnMarbleReleased;
        DraggingState.OnMarbleLevelUpdate -= OnMarbleLevelUpdate;
        AbsorbState.OnAbsorbtionBegin -= OnMarbleAbsorbionBegin;
        Channel.OnChannelUpdate -= OnMarbleAbsorbed;
    }

    private void OnMarbleGrabbed()
    {
        OnMarbleSelect.Post(gameObject);
    }
    private void OnMarbleReleased()
    {
        OnMarbleRelease.Post(gameObject);
    }
    private void OnMarbleLevelUpdate(MarbleData data, int arg2)
    {
        if (arg2 == 1)
            OnLevelTwo.Post(gameObject);
        else if (arg2 == 2)
            OnLevelThree.Post(gameObject);
    }
    private void OnMarbleAbsorbionBegin()
    {
        OnAbsrobtionBegin.Post(gameObject);
    }
    private void OnMarbleAbsorbed(int arg1, Mood mood, float arg3)
    {
        OnMorphBegin.Post(gameObject);
    }
}
