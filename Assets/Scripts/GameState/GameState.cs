using System;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static event Action<EGameState,EGameState> OnGameStateUpdate;
    static public EGameState State { get { return currentState; } }
    private static EGameState currentState;

    public void SetGameState(EGameState state)
    {
        OnGameStateUpdate?.Invoke(currentState,state);
        currentState = state;
    }
}

[Serializable]
public enum EGameState
{
    intro, 
    game, 
    info
}