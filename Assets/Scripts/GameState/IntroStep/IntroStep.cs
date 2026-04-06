using System;
using UnityEngine.Events;

[Serializable]
public abstract class IntroStep
{
    public UnityEvent OnEnterEvents = new UnityEvent();
    public UnityEvent OnExitEvents = new UnityEvent();
    public virtual void EnterState() 
    {
        OnEnterEvents?.Invoke();
    }
    public abstract bool Update();
    public virtual void ExitState()
    {
        OnExitEvents?.Invoke();
    }
}
