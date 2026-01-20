using System.Collections;
using UnityEngine;

public class MarbleBehaviour : MonoBehaviour
{
    private Collider coll;
    [HideInInspector] public Transform trans;
    [HideInInspector] public Color color;
    [HideInInspector] public int index;
    [HideInInspector] public MarbleState state {  get; private set; }
    [HideInInspector] public float lerpValue = 0;
    [HideInInspector] public Material mat;

    [Header("Animation")]
    [SerializeField] AnimationCurve scalingCurve;
    [SerializeField] float scaleOffset;
    [SerializeField] float scaleSpeed;
    private float sizeLerp;


    private float initialScale;


    public Vector3 OnReleasePos { get; private set; }

    Renderer rend;

    private void Awake()
    {
        coll = GetComponent<Collider>();
        trans = transform;
        rend = trans.GetComponent<Renderer>();
        mat = new(rend.material);
        rend.material = mat;
        initialScale = trans.localScale.x;
    }

    public void Initialize(Color color, int index)
    {
        this.color = color;
        this.index = index;
        mat.color = color;
        SetState(MarbleState.lerpIn);
    }

    public void SetState(MarbleState newState)
    {
        lerpValue = 0;
        //Enter
        switch (newState)
        {
            case MarbleState.lerpIn:
                coll.enabled = false;
                break;
            case MarbleState.dragged:
                coll.enabled = false; 
                break;
            case MarbleState.idle:
                coll.enabled = true;
                break;
            case MarbleState.recover: 
                coll.enabled = false;
                break;
            case MarbleState.consummed:
                coll.enabled = false;
            break;
        }

        state = newState;
    }

    public void OnGrabbed()
    {
        SetState(MarbleState.dragged);
        StopAllCoroutines();
        StartCoroutine(Expand());
    }

    public void OnRelease()
    {
        OnReleasePos = trans.position;
        StopAllCoroutines();
        StartCoroutine(Shrink());
    }

    private IEnumerator Expand()
    {
        float targetScale;
        while (sizeLerp < 1)
        {
            sizeLerp += Time.deltaTime * scaleSpeed;
            targetScale = initialScale + scaleOffset *  scalingCurve.Evaluate((sizeLerp));
            trans.localScale = new Vector3(targetScale, targetScale, targetScale);
            yield return null;
        }

        targetScale = initialScale + scaleOffset;
        trans.localScale = new Vector3(targetScale, targetScale, targetScale);

    }

    private IEnumerator Shrink()
    {
        float targetScale;
        while (sizeLerp > 0)
        {
            sizeLerp -= Time.deltaTime * scaleSpeed;
            targetScale = initialScale + scaleOffset * scalingCurve.Evaluate((sizeLerp));
            trans.localScale = new Vector3(targetScale, targetScale, targetScale);
            yield return null;
        }
        trans.localScale = new Vector3(initialScale, initialScale, initialScale);
    }
}

public enum MarbleState
{
    dragged,
    idle, 
    recover, 
    consummed, 
    lerpIn
}