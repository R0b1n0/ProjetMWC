using System;
using UnityEngine;

public class Credits : MonoBehaviour
{
    [SerializeField] GameState gameState;
    [SerializeField] Animator creditAnimation;

    public void TriggerCredits()
    {
        if (GameState.State == EGameState.game)
        {
            gameState.SetGameState(EGameState.credit);
            creditAnimation.SetBool("Display", true);
        }
    }

    public void CloseCredit()
    {
        creditAnimation.SetBool("Display", false);
        gameState.SetGameState(EGameState.game);
    }
}
