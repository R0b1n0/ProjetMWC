using System;
using System.Collections;
using UnityEngine;

public class MarbleData : MonoBehaviour
{
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

    [Header("Lerp in")]
    [SerializeField] AnimationCurve lerpInCurve;
    public AnimationCurve LerpInCurve { get { return lerpInCurve; } }

    [Header("Recover")]
    [SerializeField] AnimationCurve recoverCurve;
    public AnimationCurve RecoverCurve { get { return recoverCurve; } }

    float OnInitScale;
    public float defaultScale { get; private set; }

    public float speed;
    public Vector3 direction;

    public Vector3 OnReleasePos { get; private set; }

    Renderer rend;

    public static event Action<MarbleData> RenderAura;
    public static event Action<MarbleData> StopAuraRender;

    private MarbleStateBehaviour stateBh;

    public int maxLoadValue  { get { return 2; } }
    public float currentLoadValue = 0;
    private void Awake()
    {
        trans = transform;
        rend = trans.GetComponent<Renderer>();
        mat = new(rend.material);
        rend.material = mat;
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
        stateBh = new LerpInState(this);
    }

    private void Update()
    {
        if (stateBh == null)
            return;

        MarbleStateBehaviour newState = stateBh.Update();
        if (newState != stateBh)
        {
            stateBh.ExitState();
            newState.EnterState();
            stateBh = newState;
        }
    }

    public void UpdateOnCanvaRescale(float newDefaultScale)
    {
        defaultScale = newDefaultScale;
        if (state == MarbleState.idle)
        {
            trans.localScale = new Vector3(defaultScale, defaultScale, defaultScale);
        }
    }

    /*public void SetState(MarbleState newState)
    {
        lerpValue = 0;
        //Enter
        switch (newState)
        {
            case MarbleState.lerpIn:
                StopAuraRender?.Invoke(this);
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
                OnRecoverBegin();
                break;
            case MarbleState.consummed:
                coll.enabled = false;
                //Start shrinking

            break;
        }

        state = newState;
    }*/

    public void OnLevelUpdate(int newLevel)
    {
        StopAllCoroutines();
        StartCoroutine(Expand(newLevel));
    }

    public void OnGrabbed()
    {
        StopAllCoroutines();
        RenderAura?.Invoke(this);
        StartCoroutine(Expand(0));
    }
    public void OnRecovered()
    {
        StopAuraRender?.Invoke(this);
    }
    public void OnRecoverBegin()
    {
        OnReleasePos = trans.position;
        StopAllCoroutines();
        StartCoroutine(RestoreDefaultScale());
    }

    #region Animation
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

    private IEnumerator RestoreDefaultScale()
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
    #endregion
}

public enum MarbleState
{
    empty,
    dragged,
    idle, 
    recover, 
    consummed, 
    lerpIn,
    thrown
}