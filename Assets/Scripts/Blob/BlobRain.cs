using System;
using System.Collections.Generic;
using UnityEngine;

public class BlobRain : MonoBehaviour
{
    //Shader was set to have a maximum of 32 parts 
    //9 are reserved for the main parts and marbles
    [Header("Cloud parameters")]
    [SerializeField] int maxRainDropCount = 20;
    [SerializeField] float minDropDelay;
    [SerializeField] float maxDropDelay;
    [HideInInspector] public List<Part> rainDropPart { get; private set; }
    private List<RainDrop> rainDrops = new List<RainDrop>();

    [Header("Drop appearance")]
    [SerializeField] float maxRadius;
    [SerializeField] float minRadius;
    [SerializeField] AnimationCurve radiusCurve;
    [SerializeField] float dropFallingSpeed;
    [SerializeField] float dropScalingSpeed;

    float lastDropDt;

    [SerializeField] float delayBetweenDrops = 0f;
    bool enableDrops = false;
    public float rainIntensity {  get; private set; }

    class RainDrop
    {
        public Part parent;
        public Part part;
        public Vector2 freefallOg;
        public float originRadius;
        public bool freeFall = false;
        public static float freefallTreshold;
    }

    private void Awake()
    {
        rainDropPart = new List<Part>();
        Keyframe[] keys = new Keyframe[3];
        radiusCurve.GetKeys(keys);
        RainDrop.freefallTreshold = keys[1].time;
    }

    public void UpdateRainDrops(List<Part> blobParts)
    {
        if (enableDrops)
            CreateNewDrops(blobParts);

        UpdateDropsPos();
        TrimRainDrops();
    }
    public void SetRainLevel(float i)
    {
        enableDrops = i > 0;
        delayBetweenDrops = maxDropDelay - i * (maxDropDelay - minDropDelay);
        rainIntensity = i;
    }

    private void CreateNewDrops(List<Part> blobParts)
    {
        lastDropDt += Time.deltaTime;

        if (lastDropDt > delayBetweenDrops && rainDropPart.Count < maxRainDropCount)
        {
            // create a new Drop
            Part drop = new Part();
            drop.radius = UnityEngine.Random.Range(minRadius, maxRadius);

            int parentIndex = UnityEngine.Random.Range(0, blobParts.Count - 1);
            drop.currentPos = blobParts[parentIndex].currentPos - new Vector2(0, blobParts[parentIndex].radius) ;
            drop.destination = drop.currentPos + new Vector2(0, -1f);
            drop.origin = drop.currentPos;
            drop.lerpSpeed = dropFallingSpeed;
            rainDropPart.Add(drop);
            rainDrops.Add(new RainDrop
            {
                parent = blobParts[parentIndex],
                part = rainDropPart[rainDropPart.Count - 1],
                originRadius = drop.radius
            });
            lastDropDt = 0;
        }
    }
    private void UpdateDropsPos()
    {
        foreach (RainDrop rainDrop in rainDrops)
        {
            rainDrop.part.radius = radiusCurve.Evaluate(rainDrop.part.lerpPhase) * rainDrop.originRadius;

            if (!rainDrop.freeFall)
            {
                rainDrop.part.lerpPhase += Time.deltaTime * dropScalingSpeed;

                rainDrop.part.currentPos = rainDrop.parent.currentPos - new Vector2(0, rainDrop.parent.radius + rainDrop.part.radius);

                if (rainDrop.part.lerpPhase > RainDrop.freefallTreshold)
                {
                    rainDrop.freeFall = true;
                    rainDrop.part.origin = rainDrop.part.currentPos;
                    rainDrop.part.destination = rainDrop.part.currentPos - new Vector2(0, 1f);
                }
            }
            else
            {
                rainDrop.part.lerpPhase += Time.deltaTime * dropFallingSpeed;
                float dropLerp = (rainDrop.part.lerpPhase - RainDrop.freefallTreshold) / (1 - RainDrop.freefallTreshold);
                rainDrop.part.alpha = 1 - dropLerp;
                rainDrop.part.currentPos = Vector2.Lerp(rainDrop.part.origin, rainDrop.part.destination, dropLerp);
            }
        }
    }
    private void TrimRainDrops()
    {
        //Remove old drops
        for (int i = rainDropPart.Count - 1; i >= 0; i--)
        {
            if (rainDropPart[i].lerpPhase > 1)
            {
                rainDropPart.RemoveAt(i);
            }
        }
    }
}