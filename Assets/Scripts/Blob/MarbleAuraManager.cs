using System.Collections.Generic;
using UnityEngine;

public class MarbleAuraManager 
{
    public List<MarbleAuraRenderState> marbles2Render { get; private set; }

    public MarbleAuraManager()
    {
        marbles2Render = new();
        MarbleData.RenderAura += OnRenderCall;
        MarbleData.StopAuraRender += StopRendering;
    }

    ~MarbleAuraManager()
    {
        MarbleData.RenderAura -= OnRenderCall;
        MarbleData.StopAuraRender -= StopRendering;
    }
    void OnRenderCall(MarbleData marble, bool instant = false)
    {
        //Check if the marble is already here
        if (ContainMarbleData(marble, out MarbleAuraRenderState state))
        {
            state.render = true;

            if (instant)
                state.scale = 1;
        }
        else
        {
            marbles2Render.Add(new MarbleAuraRenderState(marble));

            if (instant)
                marbles2Render[marbles2Render.Count - 1].scale = 1;
        }
    }
    void StopRendering(MarbleData marble, bool instant = false)
    {
        if (ContainMarbleData(marble, out MarbleAuraRenderState marbleState))
        {
            marbleState.render = false;
            if (instant)
                marbleState.scale = 0;
        }
    }
    public void ProcessMarblesAura()
    {
        foreach (MarbleAuraRenderState state in marbles2Render)
        {
            if (state.render)
            {
                //Increase aura scale
                state.scale += Time.deltaTime;
                if (state.scale >= 1)
                    state.scale = 1;
            }
            else
            {
                //Decrease aura and remove if needed
                state.scale -= Time.deltaTime;
                if (state.scale <= 0)
                    state.obsolete = true;
            }
        }

        for (int i = marbles2Render.Count - 1; i >= 0; i--)
        {
            if (marbles2Render[i].obsolete)
                marbles2Render.RemoveAt(i);
        }
    }

    private bool ContainMarbleData(MarbleData marble, out MarbleAuraRenderState state)
    {
        for (int i = 0; i < marbles2Render.Count; i++)
        {
            if (marbles2Render[i].marble == marble)
            {
                state = marbles2Render[i];
                return true;
            }
        }
        state = null;
        return false;
    }
}

public class MarbleAuraRenderState
{
    public float scale;
    public bool render;
    public bool obsolete;
    public MarbleData marble;

    public MarbleAuraRenderState(MarbleData newMarble = null)
    {
        scale = 0f;
        render = true;
        obsolete = false;
        marble = newMarble;
    }
}