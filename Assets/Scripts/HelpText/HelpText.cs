using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HelpText : MonoBehaviour
{
    [SerializeField] GameState gameState;
    [SerializeField] List<Page> pages = new List<Page>();
    [SerializeField] GameObject paragraphePb;
    private List<ParagraphDrawer> drawers = new List<ParagraphDrawer>();
    [SerializeField] RectTransform drawerHolder;
    private int currentPage = 0;
    private bool displaying = false;
    private bool lockInputs = false;

    private void Start()
    {
        InputManager.instance.OnTouchStartEvent += OnTouchStarted;
    }
    private void OnDestroy()
    {
        InputManager.instance.OnTouchStartEvent -= OnTouchStarted;
    }

    private void OnTouchStarted()
    {
        if (!displaying || lockInputs)
            return;
        
        StartCoroutine(EraseAndTryDisplay());
    }

    private IEnumerator EraseAndTryDisplay()
    {
        lockInputs = true;
        foreach (ParagraphDrawer paragraphes in drawers)
        {
            paragraphes.RemoveText();
        }
        bool erasing = true;

        while (erasing)
        {
            erasing = false;
            foreach (ParagraphDrawer paragraphes in drawers)
            {
                if (!paragraphes.cleared)
                {
                    erasing = true;
                    continue;
                }
            }
            yield return null;
        }

        if (currentPage >= pages.Count )
        {
            //Stop Displaying 
            displaying = false;
            gameState.SetGameState(EGameState.game);
        }
        else
        {
            DisplayCurrentPage();
        }

        lockInputs = false;
    }

    //Triggered by the help button 
    public void StartText()
    {
        if (lockInputs)
            return;

        gameState.SetGameState(EGameState.info);
        currentPage = 0;
        DisplayCurrentPage();
    }

    private void DisplayCurrentPage()
    {
        displaying = true;
        Page page = pages[currentPage];

        //Create page if needed 
        if (page.paragraphs.Count > drawers.Count)
        {
            for (int i = 0; i < (page.paragraphs.Count - drawers.Count); i++)
                drawers.Add(CreateParagraphe());
        }

        for (int i = 0; i < page.paragraphs.Count; i++)
        {
            drawers[i].DisplayText(page.paragraphs[i]);
        }
        currentPage++;
    }

    private ParagraphDrawer CreateParagraphe()
    {
        return Instantiate(paragraphePb, drawerHolder).GetComponent<ParagraphDrawer>();
    }
}

[Serializable]
public class Page
{
    public List<Paragraph> paragraphs;
}

[Serializable]
public class Paragraph
{
    public string text;
    public Color color;
    public RectTransform target;
}