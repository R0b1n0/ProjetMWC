using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class ParagraphDrawer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    public bool isEmpty { get; private set; }
    [SerializeField] float spellDelay;
    [SerializeField] float eraseDelay;
    [SerializeField] string test;


    private void Start()
    {
        RemoveText();
    }

    public void DisplayText()
    {
        StartCoroutine(SpellText(test));
    }
    private IEnumerator SpellText(string textToDisplay)
    {
        int currentChar = 0;

        while (currentChar < textToDisplay.Length)
        {
            if (textToDisplay[currentChar] == '<')
            {
                while (textToDisplay[currentChar] != '>')
                {
                    text.text += textToDisplay[currentChar];
                    currentChar++;
                }
                //Add the '>'
                text.text += textToDisplay[currentChar];
                currentChar++;
                yield return null;
            }
            else
            {
                text.text += textToDisplay[currentChar];
                currentChar++;
                yield return new WaitForSeconds(spellDelay);
            }
        }
    }
    private void RemoveText()
    {
        StartCoroutine(EraseText());
    }
    private IEnumerator EraseText()
    {
        int currentChar = text.text.Length - 1;

        while (currentChar >= 0)
        {
            if (text.text[currentChar] == '>')
            {
                while (text.text[currentChar] != '<')
                {
                    text.text = text.text.Remove(currentChar,1);
                    currentChar--;
                }
                text.text = text.text.Remove(currentChar, 1);
                currentChar--;
                yield return null;
            }
            else
            {
                text.text = text.text.Remove(currentChar, 1);
                currentChar--;
                yield return new WaitForSeconds(eraseDelay);
            }
        }
    }
}
