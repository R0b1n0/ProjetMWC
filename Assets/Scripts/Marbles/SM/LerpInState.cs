using UnityEngine;

public class LerpInState : MarbleStateBehaviour
{
    Vector3 start;
    Vector3 end;
    Vector3 s2f;
    float t;

    public LerpInState(MarbleData marble) : base(marble)
    {
        end = MarbleManager.instance.GetSlotPos(marble.index);
        start = MarbleManager.instance.GetLerpInStartPos(marble.index);
        s2f = end - start;
    }

    public override void EnterState()
    {
    }

    public override void ExitState()
    {
    }

    public override MarbleStateBehaviour Update()
    {
        marble.trans.position = start + s2f * marble.LerpInCurve.Evaluate(t);
        t += Time.deltaTime / 3;

        //Reached Destination
        if (t >= 1)
        {
            ExitState();
            return new IdleState(marble);
        } 

        return this;
    }
}
