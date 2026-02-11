using System;
using System.Collections;
using UnityEngine;

public class Utils : MonoBehaviour
{
    [SerializeField] RectTransform canva;

    public static float screen2World {  get; private set; }
    public static event Action OnScreenRescale;

    private void Awake()
    {
        StartCoroutine(ProcessScreenScaler());
    }

    private void OnRectTransformDimensionsChange()
    {
        StartCoroutine(ProcessScreenScaler());
    }
   
    private void ComputeScreenToWorldFactor()
    {
        Vector3[] holderCornersW = new Vector3[4];
        canva.GetWorldCorners(holderCornersW);
        Vector3[] holderCornersS = new Vector3[4];
        canva.GetLocalCorners(holderCornersS);

        float worldD = Vector3.Distance(holderCornersW[0], holderCornersW[2]);
        float screenD = Vector3.Distance(holderCornersS[0], holderCornersS[2]);

        screen2World = worldD / screenD;
        OnScreenRescale?.Invoke();
    }

    IEnumerator ProcessScreenScaler()
    {
        yield return new WaitForEndOfFrame();
        ComputeScreenToWorldFactor();
    }
}