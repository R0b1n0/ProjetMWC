using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiFadeIn : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI helpTextBtn;
    [SerializeField] Image creditBtn;
    private void Awake()
    {
        GameState.OnGameStateUpdate += DisplayBtn;
    }

    private void OnDestroy()
    {
        GameState.OnGameStateUpdate -= DisplayBtn;
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
            helpTextBtn.color = new Color(helpTextBtn.color.r, helpTextBtn.color.g, helpTextBtn.color.b, t);
            creditBtn.color = new Color(1, 1, 1, t);
            yield return null;
        }
        creditBtn.color = new Color(1, 1, 1, 1);
        helpTextBtn.color = new Color(helpTextBtn.color.r, helpTextBtn.color.g, helpTextBtn.color.b, 1);
    }
}
