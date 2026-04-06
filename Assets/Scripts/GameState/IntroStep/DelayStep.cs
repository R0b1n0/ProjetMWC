using System;
using UnityEngine;

[Serializable]
public class DelayStep : IntroStep
{
    public float delay;
    float elapsedTime = 0;

    public override bool Update()
    {
        elapsedTime += Time.deltaTime;
        return elapsedTime > delay;
    }
}
