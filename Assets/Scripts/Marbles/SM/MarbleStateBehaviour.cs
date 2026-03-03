public abstract class MarbleStateBehaviour
{
    protected MarbleData marble;
    protected MarbleStateBehaviour(MarbleData marble)
    {
        this.marble = marble;
    }
    public abstract void EnterState();
    public abstract void ExitState();
    public abstract MarbleStateBehaviour Update();
}
