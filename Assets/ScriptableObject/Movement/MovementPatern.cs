using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementPatern", menuName = "Scriptable Objects/MovementPatern")]
public abstract class MovementPatern : ScriptableObject
{
    public float speed;

    public abstract void UpdatePos(List<Vector3> points);
}
