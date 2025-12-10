using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BlobManager : MonoBehaviour
{
    [SerializeField] Material blobMaterial;
    [SerializeField] List<CircleData> circles = new List<CircleData>();

    private void Update()
    {
        blobMaterial.SetFloat("_UnityTime", Time.time);

        int circleCount = circles.Count;
        blobMaterial.SetInt("_CircleCount", circleCount);

        Vector4[] rola = new Vector4[circleCount];
        float[] rotationSpeed = new float[circleCount];

        for (int i = 0; i < circleCount; i++)
        {
            rola[i] = new Vector4(
                circles[i].radius,
                circles[i].orbitRadius,
                circles[i].lerpSpeed,
                circles[i].angleOffset);
            rotationSpeed[i] = circles[i].rotationSpeed;
        }

        //Awfull af way to pass data 
        blobMaterial.SetVectorArray("_Circles", rola);
        blobMaterial.SetFloatArray("_RotationSpeeds", rotationSpeed);
    }
}

[Serializable]
public struct CircleData
{
    public float radius;
    public float orbitRadius;
    public float lerpSpeed;
    public float angleOffset;
    public float rotationSpeed;
}