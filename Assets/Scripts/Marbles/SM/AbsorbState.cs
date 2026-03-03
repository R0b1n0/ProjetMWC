using UnityEngine;

public class AbsorbState : MarbleStateBehaviour
{
    float absorbtionSpeed = 0.7f;
    float absorb;
    float startScale;
    float l = 0;

    public AbsorbState(MarbleData marble) : base(marble)
    {
        startScale = marble.trans.localScale.x;
    }

    public override void EnterState()
    {
    }

    public override void ExitState()
    {
        //TODO Stop rendering aura
        //Set scale and color back 2 default 
        float marbleDefaultScale = marble.defaultScale;
        marble.trans.localScale = new Vector3(marbleDefaultScale, marbleDefaultScale, marbleDefaultScale);
        marble.mat.color = marble.ogColor;
    }

    public override MarbleStateBehaviour Update()
    {
        l += Time.deltaTime * absorbtionSpeed;

        float scale = startScale - startScale * l;
        marble.trans.localScale = new Vector3 (scale, scale, scale);

        if (l > 1)
        { 
            return new LerpInState(marble);
        }

        return this;
    }
}
