using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobManager : MonoBehaviour
{
    [SerializeField] Material blobMaterial;
    [SerializeField] List<CircleData> circles = new List<CircleData>();

    [SerializeField] Color blobEdgeColor;
    [SerializeField] Color blobInnerColor;

    private float ogAnger = 1f;
    private float anger = 1f;
    private float angerTarget = 5;
    private bool goingAngry = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(RiseAnger(goingAngry));
            goingAngry = !goingAngry;
        }

        blobMaterial.SetFloat("_UnityTime", Time.time);
        blobMaterial.SetColor("_InnerColor", blobInnerColor);
        blobMaterial.SetColor("_EdgeColor", blobEdgeColor);

        int circleCount = circles.Count;
        blobMaterial.SetInt("_CircleCount", circleCount);

        Vector4[] rola = new Vector4[circleCount];
        float[] rotationSpeed = new float[circleCount];

        for (int i = 0; i < circleCount; i++)
        {
            circles[i].lerpPhase += (circles[i].lerpSpeed * anger) * Time.deltaTime;

            rola[i] = new Vector4(
                circles[i].radius,
                circles[i].orbitRadius,
                circles[i].lerpPhase,
                circles[i].angleOffset);
            rotationSpeed[i] = circles[i].rotationSpeed;
        }

        //Awfull af way to pass data 
        blobMaterial.SetVectorArray("_Circles", rola);
        blobMaterial.SetFloatArray("_RotationSpeeds", rotationSpeed);
    }

    IEnumerator RiseAnger(bool goingAngry)
    {
        float elapsed = 0;
        bool angwy = goingAngry;

        while (elapsed < 1)
        {
            elapsed += Time.deltaTime/3;
            anger = Mathf.Lerp(angwy ? ogAnger: angerTarget, angwy ? angerTarget : ogAnger, elapsed);
            yield return null;
        }
    }

    public void SetAnger(float angerValue)
    {
        anger = angerValue;
    }
}

[Serializable]
public class CircleData
{
    public float radius;
    public float orbitRadius;
    public float lerpSpeed;
    public float angleOffset;
    public float rotationSpeed;
    [HideInInspector]public float lerpPhase;
}