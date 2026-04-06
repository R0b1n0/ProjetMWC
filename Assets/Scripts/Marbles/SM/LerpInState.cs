using UnityEngine;

public class LerpInState : MarbleStateBehaviour
{
    Vector3 start;
    Vector3 end;
    Vector3 s2f;
    float t;
    float speed;

    public LerpInState(MarbleData marble) : base(marble)
    {
        end = MarbleManager.instance.GetSlotPos(marble.index);
        start = MarbleManager.instance.GetLerpInStartPos(marble.index);
        s2f = end - start;
        if (GameState.State == EGameState.intro)
            speed = 0.3f;
        else
            speed = 0.5f;
    }

    public override void EnterState()
    {
        marble.trans.position = start;
    }

    public override void ExitState()
    {
    }

    public override MarbleStateBehaviour Update()
    {
        marble.trans.position = start + s2f * marble.LerpInCurve.Evaluate(t);
        t += Time.deltaTime * speed;

        //Reached Destination
        if (t >= 1)
        {
            ExitState();
            return new IdleState(marble);
        } 

        return this;
    }
}
