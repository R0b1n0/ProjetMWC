using System.Collections;
using TMPro;
using UnityEngine;

public class HelpBtn : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    private void Start()
    {
        GameState.OnGameStateUpdate += DisplayBtn;
    }

    private void DisplayBtn(EGameState state1, EGameState state2)
    {
        if (state1 == EGameState.intro && state2 == EGameState.game)
            StartCoroutine(AlphaLerp());

    }

    private IEnumerator AlphaLerp()
    {
        float t = 0;
        while (t<=1)
        {
            t += Time.deltaTime;
            text.color = new Color(text.color.r, text.color.g, text.color.b,t);
            yield return null;
        }
        text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
    }
}
