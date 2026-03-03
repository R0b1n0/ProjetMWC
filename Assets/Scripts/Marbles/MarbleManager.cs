using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarbleManager : MonoBehaviour
{
    public static MarbleManager instance;

    [SerializeField] Camera mainCam;
    [SerializeField] GameObject marblePb;
    [SerializeField] RectTransform[] holders;
    [SerializeField] string marbleHolderTag;

    List<MarbleData> marbles = new List<MarbleData>();
    List<SlotDrawer> drawers = new List<SlotDrawer>();
    int draggedMarbleIndex;

    [Header("LerpIn")]
    [SerializeField] AnimationCurve lerpInCurve;
    [SerializeField] float lerpInSpeed;

    [Header("Idle")]
    [SerializeField] float idleSpeed;
    [SerializeField] float idleRange;

    [Header("Recover")]
    [SerializeField] AnimationCurve recoverCurve;

    [Header("Params")]
    [SerializeField] float marbleLoadingTime;
    [SerializeField, Range(0, 1)] float marbleScaleRelativToHolder;

    [Header("DragParam")]
    [SerializeField] float marbleSpeedOnDrag;
    [SerializeField] float accelerationTreshold;

    [SerializeField]
    [HideInInspector]
    private List<Mood> moodOrder;

    private float loadValue;
    private float maxLoadValue = 2;
    private Vector2 draggedMarblePreviousPos;

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        /*MarbleInputs.OnDragBegin += OnDragBegin;
        MarbleInputs.OnDragEnd += OnDragEnd;*/
        StartCoroutine(SetupOnStart());

        Utils.OnScreenRescale += UpdateMarblesOnCanvaResize;
        DraggingState.OnMarbleLevelUpdate += OnIntensityLevelUpdate;
    }

    private void OnDestroy()
    {
        /*MarbleInputs.OnDragBegin -= OnDragBegin;
        MarbleInputs.OnDragEnd -= OnDragEnd;*/
        Utils.OnScreenRescale -= UpdateMarblesOnCanvaResize;
        DraggingState.OnMarbleLevelUpdate += OnIntensityLevelUpdate;
    }

    private void Update()
    {
        //HandleMarbles();
    }

    /*private void OnDragBegin(MarbleData selectedMarble)
    {
        selectedMarble.SetState(MarbleState.dragged);
        draggedMarbleIndex = selectedMarble.index;
        loadValue = 0;
        draggedMarblePreviousPos = InputManager.instance.TouchWorldPos;
    }
    private void OnDragEnd(MarbleData selectedMarble)
    {
        selectedMarble.SetState(MarbleState.thrown);
        drawers[draggedMarbleIndex].SetSatelliteCount(1);
        draggedMarbleIndex = -1;
        Vector2 inputDir = InputManager.instance.TouchWorldPos - draggedMarblePreviousPos;
        float marbleSpeedOnRelease = (inputDir.magnitude / Time.deltaTime)/10;
        selectedMarble.speed = marbleSpeedOnRelease;
        selectedMarble.direction = inputDir.normalized;
    }*/

    private void UpdateMarblesOnCanvaResize()
    {
        Vector3[] corners = new Vector3[4];
        holders[0].GetWorldCorners(corners);
        float scale = (corners[3].x - corners[0].x) * marbleScaleRelativToHolder;

        for (int i = 0; i < marbles.Count; i++)
        {
            marbles[i].UpdateOnCanvaRescale(scale);
        }
    }
    #region Utils
    public bool TryGetHolderIndex(RectTransform rect, out int index)
    {
        index = 0;
        for (int i = 0; i < holders.Length; i++)
        {
            if(rect ==  holders[i])
            {
                index = i;
                return true;
            }
        }
        return false;
    }
    public MoodProperties GetMoodData(int index)
    {
        return EmotionParameters.Instance.GetMoodInfo(moodOrder[index]);
    }

    public Vector3 GetSlotPos(int slotIndex)
    {
        Vector3[] corners = new Vector3[4];
        RectTransform holder = holders[slotIndex];
        holder.GetWorldCorners(corners);
        return new Vector3(holder.position.x, holder.position.y, 0);
    }
    public Vector3 GetLerpInStartPos(int slotIndex)
    {
        Vector3[] corners = new Vector3[4];
        RectTransform holder = holders[slotIndex];
        holder.GetWorldCorners(corners);
        return new Vector3(holder.position.x, holder.position.y - (corners[1].y - corners[0].y), 0);
    }
    #endregion

    /*#region Animation 
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
                    DragMarble(marbles[i]);
                    break;
                case MarbleState.recover:
                    RecoverMarble(marbles[i]);
                    break;
                case MarbleState.thrown:
                    ThrowMarble(marbles[i]);
                    break;
                case MarbleState.consummed:
                    AbsorbeMarble(marbles[i]);
                    break;
                case MarbleState.lerpIn:
                    StartCoroutine(LerpIn(new List<MarbleData> { marbles[i] }));
                    marbles[i].SetState(MarbleState.empty);
                    break;
            }
        }
    }
    private void IdleMarble(MarbleData marble)
    {
        marble.trans.position = new Vector3(holders[marble.index].position.x, holders[marble.index].position.y + Mathf.Sin(marble.lerpValue) * idleRange, 0);
        marble.lerpValue += Time.deltaTime * idleSpeed;
    }
    private void DragMarble(MarbleData marble)
    {
        Vector2 randomOffset = new Vector2(Random.Range(-0.02f, 0.02f), Random.Range(-0.02f, 0.02f));
        float offsetFactor = loadValue / maxLoadValue;

        //Detect slot under cursor
        Ray ray = Camera.main.ScreenPointToRay(InputManager.instance.TouchScreenPos);
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);

        if (hit2D.collider != null && 
            hit2D.transform.CompareTag(marbleHolderTag) && 
            hit2D.transform.TryGetComponent(out RectTransform holder) &&
            TryGetHolderIndex(holder, out int holderIndex) &&
            holderIndex == marble.index)
        {
            LoadMarble();
        }


        Vector2 inputPos = InputManager.instance.TouchWorldPos + randomOffset * offsetFactor;
        Vector3 target = new Vector3(inputPos.x, inputPos.y, -2);
        float d2Target = Vector3.Distance(target, marble.trans.position);
        float stepDistance = Time.deltaTime * marbleSpeedOnDrag * Mathf.Max(d2Target, accelerationTreshold);
        draggedMarblePreviousPos = marble.trans.position;

        //Lerp without overshooting
        if (d2Target <= stepDistance)
        {
            marble.transform.position = target;
        }
        else
        {
            marble.trans.position = marble.trans.position + (target - marble.trans.position).normalized * stepDistance;
        }
    }
    private void RecoverMarble(MarbleData marble)
    {
        RectTransform holder = holders[marble.index];
        Vector3 destination = new Vector3(holder.position.x, holder.position.y, 0);

        marble.trans.position = marble.OnReleasePos + (destination - marble.OnReleasePos) * recoverCurve.Evaluate(marble.lerpValue);
        marble.lerpValue += (Time.deltaTime / Vector3.Distance(marble.OnReleasePos, destination)) * 10; //Normalize and speed it up 

        if (marble.lerpValue >= 1)
        {
            marble.lerpValue = 0;
            marble.SetState(MarbleState.idle);
        }
    }
    private void ThrowMarble(MarbleData marble)
    {
        Vector2 viewportPos = Utils.World2ViewPort(marble.trans.position);
        Vector2 uvPos = Utils.World2UV(marble.trans.position);

        marble.speed -= Time.deltaTime * 3;
        //Stop the marble once it's out of view (3 is arbitrary, we just need smth above sqrt(2))
        if (viewportPos.sqrMagnitude > 3)
            marble.speed = 0;
        Vector3 movementVector = new();

        //Move towards the blob if close enough
        Vector2 closestPartPos = BlobManager.instance.GetClosestPart(uvPos);
        Vector3 v2Part = closestPartPos - uvPos;
        float d2Part = v2Part.magnitude;

        if (d2Part < 0.6f)
        {
            //Magic number, make it a variable :/ 
            movementVector += v2Part.normalized * Time.deltaTime;
            marble.speed -= Time.deltaTime * 10;
        }

        movementVector += marble.direction * (Mathf.Max(marble.speed,0) * Time.deltaTime);

        marble.trans.position += movementVector;

        marble.direction = movementVector.normalized;

        //Absorbed 
        if (d2Part < 0.005f)
        {
            marble.SetState(MarbleState.consummed);
        }

        //Back to the slot
        if (movementVector.magnitude <= 0.001f )
        {
            marble.SetState(MarbleState.recover);
        }
    }
    private void AbsorbeMarble(MarbleData marble)
    {
        Vector2 viewportPos = Utils.World2ViewPort(marble.trans.position);
        Vector2 uvPos = Utils.World2UV(marble.trans.position);
        Vector2 closestPartPos = BlobManager.instance.GetClosestPart(uvPos);

        marble.trans.position = closestPartPos;

        marble.SetState(MarbleState.lerpIn);
    }

    private void LoadMarble()
    {
        int previousWhole = Mathf.FloorToInt(loadValue);

        if (loadValue < 2)
            loadValue += Time.deltaTime / marbleLoadingTime;
        else
            loadValue = 2;

        int flooredValue = Mathf.FloorToInt(loadValue);

        if (previousWhole != flooredValue)
        {
            //New loading level 
            //OnIntensityLevelUpdate(flooredValue);
        }
    }*/
    
    private void OnIntensityLevelUpdate(MarbleData marble, int newLevel)
    {
        //Maybe this sould be in the holder drawer class
        drawers[marble.index].SetSatelliteCount(newLevel+1);
    }

#if UNITY_EDITOR
    public void TriggerLerpInAnimation()
    {
        //StartCoroutine(LerpIn(marbles));
    }
#endif
    private IEnumerator SetupOnStart()
    {
        yield return new WaitForEndOfFrame();

        Vector3[] corners = new Vector3[4];
        holders[0].GetWorldCorners(corners);
        float scale = (corners[3].x - corners[0].x) * marbleScaleRelativToHolder;
        //Instantiate the marbles 
        for (int i = 0; i < holders.Length; i++)
        {
            GameObject marble = Instantiate(marblePb);
            MarbleData marbleBh = marble.GetComponent<MarbleData>();
            marbles.Add(marbleBh);
            marbleBh.Initialize(EmotionParameters.Instance.GetMoodInfo(moodOrder[i]).marbleColor, i, scale);
            drawers.Add(holders[i].GetComponent<SlotDrawer>());

            //marble.transform.position = new Vector3(holders[i].position.x, holders[i].position.y - (corners[1].y - corners[0].y),0);

            yield return new WaitForSeconds(0.3f);
        }

        /*for (int i = 0; i < holders.Length; i++)
        {
            StartCoroutine(LerpIn(new List<MarbleData> { marbles[i] }));
            yield return new WaitForSeconds(0.3f);
        }*/
        
    }
    private IEnumerator LerpIn(List<MarbleData> marbles)
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
            //marbles[i].SetState( MarbleState.idle);
        }
    }
}
