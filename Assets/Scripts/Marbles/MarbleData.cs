using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class MarbleData : MonoBehaviour
{
    [HideInInspector] public Transform trans;
    [HideInInspector] public Color ogColor;
    [HideInInspector] public int index;
    [HideInInspector] public Material mat;
    [HideInInspector] public Mood mood;

    [Header("Scaling Animation")]
    [SerializeField] AnimationCurve scalingCurve;
    [SerializeField] float grabScaleOffset;
    [SerializeField] float levelScaleOffset;
    [SerializeField] float scaleSpeed;

    [Header("Transparency")]
    [SerializeField] float fadeTreshold;
    [SerializeField] float maxTransparency;
    public float defaultScale { get; private set; }

    [Header("Lerp in")]
    [SerializeField] AnimationCurve lerpInCurve;
    public AnimationCurve LerpInCurve { get { return lerpInCurve; } }

    [Header("Recover")]
    [SerializeField] AnimationCurve recoverCurve;
    public AnimationCurve RecoverCurve { get { return recoverCurve; } }

    [HideInInspector] public float speed;
    [HideInInspector] public Vector3 direction;
    
    public static event Action<MarbleData,bool> RenderAura;
    public static event Action<MarbleData,bool> StopAuraRender;

    private MarbleStateBehaviour stateBh;

    public int maxLoadValue  { get { return 2; } }
    [HideInInspector] public float currentLoadValue = 0;

    private void Awake()
    {
        trans = transform;
        Renderer rend = trans.GetComponent<Renderer>();
        mat = new(rend.material);
        rend.material = mat;
    }
    public void Initialize(Mood marbleMood, Color color, int index, float initScale)
    {
        mood = marbleMood;
        ogColor = color;
        this.index = index;
        mat.color = color;
        defaultScale = initScale;
        float OnInitScale = initScale;
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
        //TODO instant update on canva resize 
        /*if (state == MarbleState.idle)
        {
            trans.localScale = new Vector3(defaultScale, defaultScale, defaultScale);
        }*/
    }
    public void OnLevelUpdate(int newLevel)
    {
        StopAllCoroutines();
        StartCoroutine(Expand(newLevel));
    }
    public void SetAura(bool value, bool instantState = false)
    {
        if (value)
            RenderAura?.Invoke(this,instantState);
        else
            StopAuraRender?.Invoke(this, instantState);
    }
    
    public void FadeWithPartDistance()
    {
        Vector2 UVPos = Utils.World2UV(trans.position);
        Vector2 closestPart = BlobManager.instance.GetClosestPartPos(UVPos);
        SetTransparency(Mathf.Min(1, Mathf.Max(maxTransparency, Vector2.Distance(UVPos, closestPart) / fadeTreshold)));
    }
    public void SetTransparency(float alphaValue)
    {
        Color newMatColor = mat.color;
        newMatColor.a = alphaValue;
        mat.color = newMatColor;
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
