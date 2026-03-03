using UnityEngine;

public class IdleState : MarbleStateBehaviour
{
    float range = 0.005f;
    float speed = 2;
    float lerpValue = 0;
    Vector3 slotPos;
    bool hasBeenGrabbed = false;

    public IdleState(MarbleData marble) : base(marble)
    {
        slotPos = MarbleManager.instance.GetSlotPos(marble.index);
    }

    public override void EnterState()
    {
        MarbleInputs.OnDragBegin += OnGrabBehaviour;
    }

    public override void ExitState()
    {
        MarbleInputs.OnDragBegin -= OnGrabBehaviour;
    }

    void OnGrabBehaviour(MarbleData marbleData)
    {
        if (marble != marbleData)
            return;

        hasBeenGrabbed = true;
    }

    public override MarbleStateBehaviour Update()
    {
        if (hasBeenGrabbed)
        {
            marble.OnGrabbed();
            marble.SetAura(true);
            return new DraggingState(marble);
        }

        marble.trans.position = new Vector3(slotPos.x, slotPos.y + Mathf.Sin(lerpValue) * range, 0);
        lerpValue += Time.deltaTime * speed;
        return this;
    }
}
