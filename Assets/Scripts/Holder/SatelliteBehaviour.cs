using System.Collections;
using UnityEngine;

public class SatelliteBehaviour : MonoBehaviour
{
    public float scaleSpeed;

    public float targetScale;
    public Vector3 Pos { set => transform.position = value; }

    private void Awake()
    {
        transform.localScale = Vector3.zero;
    }

    public void ScaleUp()
    {
        StopAllCoroutines();
        StartCoroutine(ScalingUp());
    }

    public void ScaleDown()
    {
        StopAllCoroutines();
        StartCoroutine(ScalingDown());
    }

    IEnumerator ScalingUp()
    {
        while (transform.localScale.x < targetScale)
        {
            transform.localScale = transform.localScale + new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime) * scaleSpeed;
            yield return null;
        }
        transform.localScale = new Vector3(targetScale, targetScale, targetScale);
        
    }

    IEnumerator ScalingDown()
    {
        while (transform.localScale.x > 0)
        {
            transform.localScale = transform.localScale - new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime) * scaleSpeed;
            yield return null;
        }
        transform.localScale = Vector3.zero;

    }
}
