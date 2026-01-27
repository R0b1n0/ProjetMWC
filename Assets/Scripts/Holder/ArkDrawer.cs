using System;
using System.Collections.Generic;
using UnityEngine;

public class ArkDrawer : MonoBehaviour
{
    public bool isStatic;
    [SerializeField] RectTransform recTrans;

    [SerializeField] Material lineMat;
    [SerializeField] float thickness;
    [SerializeField] float resolution;
    [HideInInspector] public float radius;
    [SerializeField] float rotationSpeed;

    [SerializeField] List<Arch> data = new List<Arch>();

    List<LineRenderer> renderers = new List<LineRenderer>();

    float angleOffset = 0; //Rad

    private void Start()
    {
        for (int i = 0; i < data.Count; i++)
        {
            GameObject child = new GameObject();
            child.transform.parent = transform;
            child.layer = transform.gameObject.layer;
            renderers.Add(child.AddComponent<LineRenderer>());
            renderers[i].material = lineMat;
            renderers[i].startWidth = renderers[i].endWidth = thickness;
        }

        FillArks();
        DrawArks();
    }

    private void Update()
    {
        DrawArks();
    }

    private void DrawArks()
    {
        for (int i = 0; i < data.Count; i++)
        {
            renderers[i].startWidth = renderers[i].endWidth = thickness;

            int pointsCount = renderers[i].positionCount;

            float bottomAngle = data[i].ogRotation - data[i].angleWidth / 2;
            float stepAngleValue = data[i].angleWidth / pointsCount;
            float currentAngle = 0;

            for (int j = 0; j < pointsCount; j++)
            {
                currentAngle = angleOffset + (bottomAngle + stepAngleValue * j) * Mathf.Deg2Rad;

                renderers[i].SetPosition(j, recTrans.position + new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle)).normalized * radius);
            }

        }
    }
    private void FillArks()
    {
        for (int i = 0; i < data.Count; i++)
        {
            renderers[i].positionCount = (int)Mathf.Floor(resolution * data[i].angleWidth / 360);
        }
    }

    public void SetOffset(float degreeOffset)
    {
        angleOffset = degreeOffset;
    }
}


[Serializable]
public struct Arch
{
    public float ogRotation;
    public float angleWidth;
}