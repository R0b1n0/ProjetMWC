using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarbleManager : MonoBehaviour
{
    [SerializeField] Camera mainCam;
    [SerializeField] GameObject marblePb;
    [SerializeField] RectTransform[] holders;
    List<MarbleBehaviour> marbles = new List<MarbleBehaviour>();

    [Header("LerpIn")]
    [SerializeField] AnimationCurve lerpInCurve;
    [SerializeField] float lerpInSpeed;

    [Header("Idle")]
    [SerializeField] float idleSpeed;
    [SerializeField] float idleRange;

    [Header("Recover")]
    [SerializeField] AnimationCurve recoverCurve;

    [SerializeField] EmotionParameters moodDic;

    [SerializeField]
    [HideInInspector]
    private List<Mood> moodOrder;

    private void Start()
    {
        MarbleInputs.OnDragBegin += OnDragBegin;
        MarbleInputs.OnDragEnd += OnDragEnd;
        StartCoroutine(SetupOnStart());
    }

    private void OnDestroy()
    {
        MarbleInputs.OnDragBegin -= OnDragBegin;
        MarbleInputs.OnDragEnd -= OnDragEnd;
    }

    private void Update()
    {
        HandleMarbles();
    }

    private void OnDragBegin(MarbleBehaviour selectedMarble)
    {
        selectedMarble.SetState(MarbleState.dragged);
    }
    private void OnDragEnd(MarbleBehaviour selectedMarble)
    {
        //Check if we're above the blob 
        selectedMarble.SetState(MarbleState.recover);
    }

    #region Animation 
    private void HandleMarbles()
    {
        for (int i = 0; i < marbles.Count; i++)
        {
            
            switch (marbles[i].state)
            {
                case MarbleState.idle:
                    IdleMarble(marbles[i]);
                    break;
                case MarbleState.dragged:
                    DragMArble(marbles[i]);
                    break;
                case MarbleState.recover:
                    RecoverMarble(marbles[i]);
                    break;

            }
        }
    }
    private void IdleMarble(MarbleBehaviour marble)
    {
        marble.trans.position = new Vector3(holders[marble.index].position.x, holders[marble.index].position.y + Mathf.Sin(marble.lerpValue) * idleRange, 0);
        marble.lerpValue += Time.deltaTime * idleSpeed;
    }
    private void DragMArble(MarbleBehaviour marble)
    {
        Vector2 inputPos = InputManager.instance.TouchWorldPos;
        marble.trans.position = Vector3.MoveTowards(marble.trans.position, new Vector3(inputPos.x, inputPos.y , -2), 0.5f);
    }
    private void RecoverMarble(MarbleBehaviour marble)
    {
        RectTransform holder = holders[marble.index];
        Vector3 destination = new Vector3(holder.position.x, holder.position.y, 0);

        marble.trans.position = marble.OnReleasePos + (destination - marble.OnReleasePos) * recoverCurve.Evaluate(marble.lerpValue);
        marble.lerpValue += (Time.deltaTime / Vector3.Distance(marble.OnReleasePos,destination)) * 10; //Normalize and speed it up 

        if (marble.lerpValue >= 1)
        {
            marble.lerpValue = 0;
            marble.SetState(MarbleState.idle);
        }
    }

    public void TriggerAnimation()
    {
        StartCoroutine(LerpIn(marbles));
    }
    private IEnumerator SetupOnStart()
    {
        yield return new WaitForEndOfFrame();
        //Instantiate the marbles 
        for (int i = 0; i < holders.Length; i++)
        {
            GameObject marble = Instantiate(marblePb);
            MarbleBehaviour marbleBh = marble.GetComponent<MarbleBehaviour>();
            marbles.Add(marbleBh);
            marbleBh.Initialize(moodDic.GetMoodInfo(moodOrder[i]).marbleColor, i);

            Vector3[] corners = new Vector3[4];
            holders[i].GetWorldCorners(corners);

            marble.transform.position = new Vector3(holders[i].position.x, holders[i].position.y - (corners[1].y - corners[0].y),0);
        }

        for (int i = 0; i < holders.Length; i++)
        {
            StartCoroutine(LerpIn(new List<MarbleBehaviour> { marbles[i] }));
            yield return new WaitForSeconds(0.3f);
        }
        
    }
    private IEnumerator LerpIn(List<MarbleBehaviour> marbles)
    {
        float t = 0;

        Vector3[] start = new Vector3[marbles.Count];
        Vector3[] destination = new Vector3[marbles.Count];
        Vector3[] corners = new Vector3[4];

        for (int i = 0;i < marbles.Count;i++)
        {
            RectTransform holder = holders[marbles[i].index];
            holder.GetWorldCorners(corners);
            start[i] = new Vector3(holder.position.x, holder.position .y- (corners[1].y - corners[0].y), 0);
            destination[i] = new Vector3( holder.position.x, holder.position.y,0);
        }

        while(t<=1)
        {
            for (int i = 0; i < marbles.Count; i++)
            {
                marbles[i].trans.position = start[i] + (destination[i] - start[i]) * lerpInCurve.Evaluate(t);
            }
            t += Time.deltaTime / 3;
            yield return null;
        }

        for (int i = 0; i < marbles.Count; i++)
        {
            marbles[i].trans.position = destination[i];
            marbles[i].lerpValue = 0;
            marbles[i].SetState( MarbleState.idle);
        }
    }
    #endregion
}

public class Marble
{
    public Collider collider;
    public Transform trans;
    public Color color;
    public int index;
    public bool idling = false;
    public float idleValue = 0;
    public Material mat;

    public Marble(Color color, int index, Transform transform)
    {
        trans = transform;
        collider = trans.GetComponent<Collider>();        
        this.color = color;
        this.index = index;
        Renderer renderer = trans.GetComponent<Renderer>();
        mat = new (renderer.material);
        mat.color = color;
        renderer.material = mat;
    }
}