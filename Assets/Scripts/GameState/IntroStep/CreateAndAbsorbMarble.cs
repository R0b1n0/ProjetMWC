using System;


[Serializable]
public class CreateAndAbsorbMarble : IntroStep
{
    public Mood mood;
    private bool done = false;

    public override void EnterState()
    {
        base.EnterState();
        Channel.OnChannelUpdate += OnMarbleAbsorbed;
        MarbleManager.instance.LerpMarbleIn(mood);
    }

    private void OnMarbleAbsorbed(int arg1, Mood mood, float arg3)
    {
        done = mood == this.mood;
    }

    public override bool Update()
    {
        return done;
    }

    public override void ExitState()
    {
        base.ExitState();
        Channel.OnChannelUpdate -= OnMarbleAbsorbed;
    }
}
