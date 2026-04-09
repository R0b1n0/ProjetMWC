using System.Collections;
using TMPro;
using UnityEngine;

public class ParagraphDrawer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    public bool isEmpty { get; private set; }
    [SerializeField] float spellDelay;
    [SerializeField] float eraseDelay;
    public bool cleared { get { return text.text.Length == 0; } }

    public void DisplayText(Paragraph toDisplay)
    {
        StartCoroutine(SpellText(toDisplay));
    }
    private IEnumerator SpellText(Paragraph para)
    {
        int currentChar = 0;
        text.color = para.color;
        transform.position = para.target.position;

        while (currentChar < para.text.Length)
        {
            if (para.text[currentChar] == '<')
            {
                while (para.text[currentChar] != '>')
                {
                    text.text += para.text[currentChar];
                    currentChar++;
                }
                //Add the '>'
                text.text += para.text[currentChar];
                currentChar++;
                yield return null;
            }
            else
            {
                text.text += para.text[currentChar];
                currentChar++;
                yield return new WaitForSeconds(spellDelay);
            }
        }
    }
    public void RemoveText()
    {
        StopAllCoroutines();
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
