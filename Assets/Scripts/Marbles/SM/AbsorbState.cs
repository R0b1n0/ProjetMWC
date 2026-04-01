using System;
using UnityEngine;

public class AbsorbState : MarbleStateBehaviour
{
    public static event Action<Mood, float> OnMarbleAbsorbtion;
    public static event Action OnAbsorbtionBegin;

    float absorbtionSpeed = 0.7f;
    float startScale;
    float l = 0;
    Part closestP;

    public AbsorbState(MarbleData marble, Part closetPart) : base(marble)
    {
        startScale = marble.trans.localScale.x;
        closestP = closetPart;
    }

    public override void EnterState()
    {
        OnAbsorbtionBegin?.Invoke();
    }

    public override void ExitState()
    {
        OnMarbleAbsorbtion?.Invoke(marble.mood, marble.currentLoadValue / marble.maxLoadValue);
        float marbleDefaultScale = marble.defaultScale;
        marble.trans.localScale = new Vector3(marbleDefaultScale, marbleDefaultScale, marbleDefaultScale);
        marble.mat.color = marble.ogColor;
        marble.SetAura(false,true);
        marble.SetTransparency(1);
    }

    public override MarbleStateBehaviour Update()
    {
        l += Time.deltaTime * absorbtionSpeed;

        float scale = startScale - startScale * l;
        marble.trans.localScale = new Vector3 (scale, scale, scale);

        //Stay on the closest part
        Vector2 closestPartUVPos = BlobRenderer.instance.GetClosestPartPos(closestP.currentPos);
        marble.trans.position = Utils.UV2World(closestPartUVPos);

        if (l > 1)
        {
            return new LerpInState(marble);
        }

        return this;
    }
}
