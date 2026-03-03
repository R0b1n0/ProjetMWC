using UnityEngine;

public class ThrownState : MarbleStateBehaviour
{
    public ThrownState(MarbleData marble) : base(marble)
    {
    }

    public override void EnterState()
    {
    }

    public override void ExitState()
    {
    }

    public override MarbleStateBehaviour Update()
    {
        Vector2 viewportPos = Utils.World2ViewPort(marble.trans.position);
        Vector2 uvPos = Utils.World2UV(marble.trans.position);

        marble.speed -= Time.deltaTime * 3;
        //Stop the marble once it's out of view (3 is arbitrary, we just need smth above sqrt(2))
        if (viewportPos.sqrMagnitude > 3)
            marble.speed = 0;
        Vector3 movementVector = new();

        //Move towards the blob if close enough
        Vector2 closestPartPos = BlobManager.instance.GetClosestPart(uvPos);
        Vector3 v2Part = closestPartPos - uvPos;
        float d2Part = v2Part.magnitude;

        if (d2Part < 0.6f)
        {
            //Magic number, make it a variable :/ 
            movementVector += v2Part.normalized * Time.deltaTime;
            marble.speed -= Time.deltaTime * 10;
        }

        movementVector += marble.direction * (Mathf.Max(marble.speed, 0) * Time.deltaTime);

        marble.trans.position += movementVector;

        marble.direction = movementVector.normalized;

        //Absorbed 
        if (d2Part < 0.005f)
        {
            //TODO absorb the marble
            return new AbsorbState(marble);
        }

        //Back to the slot
        if (movementVector.magnitude <= 0.001f)
        {
            return new RecoverState(marble);
        }
        return this;
    }
}
