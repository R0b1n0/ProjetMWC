using System;
using System.Collections;
using UnityEngine;

public class MarbleBehaviour : MonoBehaviour
{
    private Collider coll;
    [HideInInspector] public Transform trans;
    [HideInInspector] public Color color;
    [HideInInspector] public Color ogColor;
    [HideInInspector] public int index;
    [HideInInspector] public MarbleState state {  get; private set; }
    [HideInInspector] public float lerpValue = 0;
    [HideInInspector] public Material mat;

    [Header("Scaling Animation")]
    [SerializeField] AnimationCurve scalingCurve;
    [SerializeField] float grabScaleOffset;
    [SerializeField] float levelScaleOffset;
    [SerializeField] float scaleSpeed;

    float OnInitScale;
    float defaultScale;

    public float speed;
    public Vector3 direction;

    public Vector3 OnReleasePos { get; private set; }

    Renderer rend;

    public static event Action<MarbleBehaviour> RenderAura;
    public static event Action<MarbleBehaviour> StopAuraRender;

    private void Awake()
    {
        coll = GetComponent<Collider>();
        trans = transform;
        rend = trans.GetComponent<Renderer>();
        mat = new(rend.material);
        rend.material = mat;
    }

    public void UpdateOnCanvaRescale(float newDefaultScale)
    {
        defaultScale = newDefaultScale;
        if (state == MarbleState.idle)
        {
            trans.localScale = new Vector3(defaultScale, defaultScale, defaultScale);
        }
    }

    public void Initialize(Color color, int index, float initScale)
    {
        ogColor = color;
        this.color = color;
        this.index = index;
        mat.color = color;
        OnInitScale = initScale;
        defaultScale = initScale;
        trans.localScale = new Vector3(OnInitScale, OnInitScale, OnInitScale);
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
                RenderAura?.Invoke(this);
                break;
            case MarbleState.idle:
                coll.enabled = true;
                StopAuraRender?.Invoke(this);
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

    public void OnLevelUpdate(int newLevel)
    {
        StopAllCoroutines();
        StartCoroutine(Expand(newLevel));
    }

    public void OnGrabbed()
    {
        SetState(MarbleState.dragged);
        StopAllCoroutines();
        StartCoroutine(Expand(0));
    }

    public void OnRecoverBegin()
    {
        OnReleasePos = trans.position;
        StopAllCoroutines();
        StartCoroutine(Shrink());
    }

    private IEnumerator Expand(int level)
    {
        float targetScale;
        float onScaleInitial = trans.localScale.x;
        Color onScaleColor = mat.color;
        Color targetColor = GetMarbleColor(level);
        float scaleOffset = (grabScaleOffset + level * levelScaleOffset);
        float lerp = 0;

        while (lerp < 1)
        {
            lerp += Time.deltaTime * scaleSpeed;
            targetScale = onScaleInitial + scaleOffset *  scalingCurve.Evaluate((lerp));
            trans.localScale = new Vector3(targetScale, targetScale, targetScale);
            mat.color = Color.Lerp(onScaleColor, targetColor, lerp);
            yield return null;
        }

        targetScale = onScaleInitial + grabScaleOffset;
        trans.localScale = new Vector3(targetScale, targetScale, targetScale);
        mat.color = targetColor;
    }

    private IEnumerator Shrink()
    {
        float targetScale;
        float onShrinkBeginSize = trans.localScale.x;
        Color onScaleColor = mat.color;
        float lerp = 0;

        while (lerp < 1)
        {
            lerp += Time.deltaTime * scaleSpeed;
            targetScale = onShrinkBeginSize - ( (onShrinkBeginSize - defaultScale) * lerp);
            trans.localScale = new Vector3(targetScale, targetScale, targetScale);
            mat.color = Color.Lerp(onScaleColor, ogColor, lerp);
            yield return null;
        }
        trans.localScale = new Vector3(defaultScale, defaultScale, defaultScale);
        mat.color = ogColor;
    }

    private Color GetMarbleColor(int intensityLevel)
    {
        switch (intensityLevel)
        {
            case 0:
                return MarbleManager.instance.GetMoodData(index).marbleColor;

            case 1:
                return MarbleManager.instance.GetMoodData(index).marbleColor1;

            case 2:
                return MarbleManager.instance.GetMoodData(index).marbleColor2;

            default:
                return Color.rosyBrown;
        }
    }
}

public enum MarbleState
{
    dragged,
    idle, 
    recover, 
    consummed, 
    lerpIn,
    thrown
}