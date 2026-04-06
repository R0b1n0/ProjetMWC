using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro : MonoBehaviour
{
    [SerializeReference, SubclassSelector]
    List<IntroStep> steps;
    [SerializeField] GameState gameState;
    [SerializeField] bool skipIntro;

    private void Start()
    {
        gameState.SetGameState(EGameState.intro);
        StartCoroutine(ProcessIntro());
    }

    private IEnumerator ProcessIntro()
    {
        int currentStep = 0;

        if (steps.Count > 0)
            steps[0].EnterState();

        while (currentStep < steps.Count && !skipIntro)
        {
            if (steps[currentStep].Update())
            {
                steps[currentStep].ExitState();
                currentStep++;
                if (currentStep < steps.Count)
                {
                    steps[currentStep].EnterState();
                }
            }
            yield return null;
        }
        gameState.SetGameState(EGameState.game);
    }
}
