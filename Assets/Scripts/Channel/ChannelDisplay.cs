using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ChannelDisplay : MonoBehaviour
{
    [SerializeField] GameObject marblePb;
    [SerializeField] EmotionParameters moodProps;
    GameObject marble;
    Material mat;
    [SerializeField, Range(0, 1)] float marbleScaleRelativToHolder;
    [SerializeField] AnimationCurve scaleUpAnim;
    [SerializeField] float scalingSpeed;
    [SerializeField] float animationOffset;

    public int id { get ; private set; }

    public bool empty { get; private set; }
    float animationLerp;
    float slotScaleLerp;
    [SerializeField] Animator animator;

    float maxMarbleScale;
    Vector3 maxMarbleScaleV;

    private void Start()
    {
        empty = true;
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
        maxMarbleScale = (holderCorners[3].x - holderCorners[0].x) * marbleScaleRelativToHolder;
        maxMarbleScaleV = new Vector3(maxMarbleScale, maxMarbleScale, maxMarbleScale);
        transform.GetComponent<CircleCollider2D>().radius = (holderCorners[3].x - holderCorners[0].x) /2;
        marble.transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        if (empty)
            return;

        animationLerp += Time.deltaTime;
        marble.transform.position = transform.position + new Vector3(0, Mathf.Sin(animationLerp), 0) * animationOffset;
    }

    public void Select()
    {
        animator.SetBool("Selected", true);
    }
    public void UnSelect()
    {
        animator.SetBool("Selected", false);
    }
    public void SetDisplay(Mood mood, float intensity)
    {
        StopAllCoroutines();
        StartCoroutine(LerpToNewState(moodProps.GetMoodColor(mood, intensity), empty));
        empty = false;
    }
    public void Clear()
    {
        StopAllCoroutines();
        StartCoroutine(LerpToClearState());
        empty = true;
    }
    public void SetId(int newId)
    {
        id = newId;
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
                float newScale = previousScale + ((maxMarbleScale - previousScale) * scaleT);
                marble.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
            yield return null;
        }
        mat.color = targetColor;
        marble.transform.localScale = maxMarbleScaleV;
    }
    IEnumerator LerpToClearState()
    {
        float previousScale = marble.transform.localScale.x;
        float t = (marble.transform.localScale.x / maxMarbleScale);

        while (t > 0.0f)
        {
            t -= Time.deltaTime * scalingSpeed;

            float scaleT = scaleUpAnim.Evaluate(t);
            float newScale = scaleT * maxMarbleScale;
            marble.transform.localScale = new Vector3(newScale, newScale, newScale);

            yield return null;
        }
        mat.color = Color.white;
        marble.transform.localScale = Vector3.zero;
    }
}
