using System;
using System.Collections.Generic;
using UnityEngine;

public class HelpText : MonoBehaviour
{
    [SerializeField] List<Page> pages = new List<Page>();

}

[Serializable]
public struct Page
{
    public List<Paragraph> paragraphs;
}

[Serializable]
public struct Paragraph
{
    public List<string> text;
    public Color color;
    public RectTransform target;
}