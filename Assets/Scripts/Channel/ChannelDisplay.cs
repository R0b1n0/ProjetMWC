using System.Collections;
using UnityEngine;

public class ChannelDisplay : MonoBehaviour
{
    [SerializeField] GameObject marblePb;
    [SerializeField] EmotionParameters moodProps;
    GameObject marble;
    Material mat;
    [SerializeField, Range(0, 1)] float marbleScaleRelativToHolder;
    [SerializeField] AnimationCurve scaleUpAnim;
    [SerializeField] float scalingSpeed;
    [SerializeField] float swingOffset;
    bool empty = true;
    float maxScale;
    Vector3 maxScaleV;
    float elapsed;

    private void Start()
    {
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        while (!gameObject)
            yield return null;

        yield return new WaitForEndOfFrame();

        CreateMarble();
    }

    private void CreateMarble()
    {
        marble = Instantiate(marblePb);
        marble.transform.parent = transform;

        mat = new Material(marble.GetComponent<MeshRenderer>().material);
        mat.color = Color.white;
        marble.GetComponent<MeshRenderer>().material = mat;

        marble.transform.position = transform.position;

        Vector3[] holderCorners = new Vector3[4];
        transform.GetComponent<RectTransform>().GetLocalCorners(holderCorners);
        maxScale = (holderCorners[3].x - holderCorners[0].x) * marbleScaleRelativToHolder;
        maxScaleV = new Vector3(maxScale, maxScale, maxScale);
        transform.GetComponent<CircleCollider2D>().radius = (holderCorners[3].x - holderCorners[0].x) /2;
        marble.transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        if (empty)
            return;

        elapsed += Time.deltaTime;
        marble.transform.position = transform.position + new Vector3(0, Mathf.Sin(elapsed), 0) * swingOffset;
    }

    public void SetDisplay(Mood mood, float intensity)
    {
        StopAllCoroutines();
        StartCoroutine(LerpToNewState(moodProps.GetMoodColor(mood, intensity), empty));
        empty = false;
    }

    IEnumerator LerpToNewState(Color targetColor, bool updateScale)
    {
        float t = 0;
        Color previousColor = mat.color;
        float previousScale = marble.transform.localScale.x;

        while (t < 1.0f)
        {
            t += Time.deltaTime * scalingSpeed;
            mat.color = Color.Lerp(previousColor, targetColor, t);
            if (updateScale)
            {
                float scaleT = scaleUpAnim.Evaluate(t);
                float newScale = previousScale + ((maxScale - previousScale) * scaleT);
                marble.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
            yield return null;
        }
        mat.color = targetColor;
        marble.transform.localScale = maxScaleV;
    }
}
