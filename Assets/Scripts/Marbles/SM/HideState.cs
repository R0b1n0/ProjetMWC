using UnityEngine;
using System;

public class HideState : MarbleStateBehaviour
{
    public HideState(MarbleData marble) : base(marble)
    {
    }

    public override void EnterState()
    {
        marble.trans.position = new Vector3(500,0,0);
    }

    public override void ExitState()
    {
    }

    public override MarbleStateBehaviour Update()
    {
        return this;
    }
}
