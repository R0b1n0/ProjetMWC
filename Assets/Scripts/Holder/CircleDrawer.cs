using System;
using UnityEngine;

public class CircleDrawer : MonoBehaviour
{
    public bool fill;

    public float radius;
    public float thickness;
    public int resolution; 

    public Vector3 center;
    public float degreeAngleOffset;

    [SerializeField] MeshRenderer meshRend; 


}
