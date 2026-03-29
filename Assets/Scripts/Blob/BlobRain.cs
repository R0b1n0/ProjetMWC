using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class BlobRain : MonoBehaviour
{
    //Shader was set to have a maximum of 32 parts 
    //9 are reserved for the main parts and marbles

    [SerializeField] int maxRainDropCount = 20;
    [HideInInspector] public List<Part> rainDrops { get; private set; }

    [Header("Drop appearance")]
    [SerializeField] float maxRadius;
    [SerializeField] float minRadius;
    [SerializeField] AnimationCurve dropScale;

    float lastDropDt;

    float delayBetweenDrops = 0.5f;

    private void Awake()
    {
        rainDrops = new List<Part>();
    }

    public void UpdateRainDrops(List<Part> blobParts)
    {
        lastDropDt += Time.deltaTime;

        if (lastDropDt > delayBetweenDrops && rainDrops.Count < maxRainDropCount)
        {
            // create a new Drop
            Part drop = new Part();
            drop.radius = Random.Range(minRadius, maxRadius);
            drop.currentPos = blobParts[Random.Range(0, blobParts.Count - 1)].currentPos;
            drop.destination = drop.currentPos + new Vector2(0, -1f);
            drop.origin = drop.currentPos;
            drop.lerpSpeed = 0.2f;

            rainDrops.Add(drop);
            lastDropDt = 0;
        }

        foreach (Part drop in rainDrops)
        {
            drop.currentPos = Vector2.Lerp(drop.origin, drop.destination, drop.lerpPhase);
            //drop.lerpPhase += Time.deltaTime * (drop.lerpSpeed / (Vector2.Distance(drop.origin, drop.destination) * 2));
            drop.lerpPhase += Time.deltaTime * drop.lerpSpeed;
        }

        //Remove old drops
        for (int i = rainDrops.Count-1; i >= 0; i-- )
        {
            if (rainDrops[i].lerpPhase > 1)
            {
                rainDrops.RemoveAt(i);
            }
        }
    }
}
