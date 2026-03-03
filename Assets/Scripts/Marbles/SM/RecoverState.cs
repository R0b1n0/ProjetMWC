using UnityEngine;

public class RecoverState : MarbleStateBehaviour
{
    float lerpValue = 0;
    Vector3 posOnRelease;
    Vector3 target;

    float onStartScale;
    Color startColor;

    public RecoverState(MarbleData marble) : base(marble)
    {
        posOnRelease = marble.trans.position;
        target = MarbleManager.instance.GetSlotPos(marble.index);
        onStartScale = marble.trans.localScale.x;
        startColor = marble.mat.color;
    }

    public override void EnterState()
    {
        //marble.OnRecoverBegin();
    }

    public override void ExitState()
    {
        //Stop rendering aura
        marble.OnRecovered();
    }

    public override MarbleStateBehaviour Update()
    {
        float newLerp = marble.RecoverCurve.Evaluate(lerpValue);
        lerpValue += (Time.deltaTime / Vector3.Distance(posOnRelease, target)) * 10; //Normalize and speed it up 

        marble.trans.position = posOnRelease + (target - posOnRelease) * newLerp;
        float targetScale = onStartScale - ((onStartScale - marble.defaultScale) * newLerp);
        marble.trans.localScale = new Vector3(targetScale, targetScale, targetScale);
        marble.mat.color = Color.Lerp(startColor, marble.ogColor, newLerp);

        if (lerpValue >= 1)
        {
            marble.trans.localScale = new Vector3(marble.defaultScale, marble.defaultScale, marble.defaultScale);
            marble.mat.color = marble.ogColor;
            return new IdleState(marble);
        }
        return this;
    }
}
