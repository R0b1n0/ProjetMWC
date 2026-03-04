using System;
using System.Collections;
using UnityEngine;

public class Utils : MonoBehaviour
{
    static RectTransform canva;

    public static float screen2World {  get; private set; }
    public static event Action OnScreenRescale;

    private void Awake()
    {
        canva = GetComponent<RectTransform>();
        StartCoroutine(ProcessScreenScaler());
    }

    public static Vector2 World2Screen(Vector2 worldPos)
    {
        return new Vector2(worldPos.x, worldPos.y) / screen2World;
    }
    public static Vector2 World2ViewPort(Vector2 worldPos)
    {
        Vector2 screenPos = World2Screen(worldPos);
        return new Vector2(screenPos.x / canva.rect.width, screenPos.y / canva.rect.height) * 2;
    }
    public static Vector2 World2UV(Vector2 worldPos)
    {
        Vector2 screenPos = World2Screen(worldPos);
        return new Vector2(screenPos.x / canva.rect.width, screenPos.y / canva.rect.width) * 2;
    }

    #region UIResize 
    private void OnRectTransformDimensionsChange()
    {
        if (canva && canva.gameObject && canva.gameObject.activeSelf) 
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
        while (!canva && !canva.gameObject)
            yield return null;

        yield return new WaitForEndOfFrame();

        ComputeScreenToWorldFactor();
    }
    #endregion 
}