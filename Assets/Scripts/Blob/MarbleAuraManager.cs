using System.Collections.Generic;
using UnityEngine;

public class MarbleAuraManager 
{
    public List<MarbleAuraRenderState> marbles2Render { get; private set; }

    public MarbleAuraManager()
    {
        marbles2Render = new();
        MarbleBehaviour.RenderAura += OnRenderCall;
        MarbleBehaviour.StopAuraRender += StopRendering;
    }

    ~MarbleAuraManager()
    {
        MarbleBehaviour.RenderAura -= OnRenderCall;
        MarbleBehaviour.StopAuraRender -= StopRendering;
    }
    void OnRenderCall(MarbleBehaviour marble)
    {
        //Check if the marble is already here
        if (ContainMarbleData(marble, out MarbleAuraRenderState state))
        {
            state.render = true;
        }
        else
        {
            marbles2Render.Add(new MarbleAuraRenderState(marble));
        }
    }
    void StopRendering(MarbleBehaviour marble)
    {
        if (ContainMarbleData(marble, out MarbleAuraRenderState marbleState))
        {
            marbleState.render = false;
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

    private bool ContainMarbleData(MarbleBehaviour marble, out MarbleAuraRenderState state)
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
    public MarbleBehaviour marble;

    public MarbleAuraRenderState(MarbleBehaviour newMarble = null)
    {
        scale = 0f;
        render = true;
        obsolete = false;
        marble = newMarble;
    }
}