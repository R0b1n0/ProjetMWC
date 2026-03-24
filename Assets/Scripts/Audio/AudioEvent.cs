using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioEvent : MonoBehaviour
{
    [Header("Events References")]
    [SerializeField] AK.Wwise.Event playAll;
    [SerializeField] AK.Wwise.Switch slot1;

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
        parameters.switchs[(int)marbleMood].SetValue(gameObject);
    }
}

[Serializable]
public class AudioParam
{
    public List<AK.Wwise.Switch> switchs;
}

/* 
 Todo 
struct {
Dictionnary[Mood,Switch] data [3] 
 }
*/
