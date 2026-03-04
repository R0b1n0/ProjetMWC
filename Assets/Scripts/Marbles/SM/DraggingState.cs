using System;
using UnityEngine;
public class DraggingState : MarbleStateBehaviour
{
    public static event Action<MarbleData,int> OnMarbleLevelUpdate;

    float marbleSpeedOnDrag = 8;
    float accelerationTreshold = 2;
    string marbleHolderTag = "MarbleHolder";
    private Vector2 draggedMarblePreviousPos;
    float marbleLoadingTime = 2;

    private bool released = false;

    public DraggingState(MarbleData marble) : base(marble)
    {

    }

    public override void EnterState()
    {
        MarbleInputs.OnDragEnd += OnRelease;
        draggedMarblePreviousPos = InputManager.instance.TouchWorldPos;
        marble.currentLoadValue = 0;
    }

    public override void ExitState()
    {
        MarbleInputs.OnDragEnd -= OnRelease;
    }

    private void OnRelease(MarbleData marble)
    {
        Vector2 inputDir = InputManager.instance.TouchWorldPos - draggedMarblePreviousPos;
        float marbleSpeedOnRelease = (inputDir.magnitude / Time.deltaTime) / 10;
        marble.speed = marbleSpeedOnRelease;
        marble.direction = inputDir.normalized;
        released = true;
    }

    public override MarbleStateBehaviour Update()
    {
        if (released)
        {
            OnMarbleLevelUpdate?.Invoke(marble, 0);
            return new ThrownState(marble);
        }

        Vector2 randomOffset = new Vector2(UnityEngine.Random.Range(-0.02f, 0.02f), UnityEngine.Random.Range(-0.02f, 0.02f));
        float offsetFactor = marble.currentLoadValue / marble.maxLoadValue;

        //Detect slot under cursor
        Ray ray = Camera.main.ScreenPointToRay(InputManager.instance.TouchScreenPos);
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray);

        if (hit2D.collider != null &&
            hit2D.transform.CompareTag(marbleHolderTag) &&
            hit2D.transform.TryGetComponent(out RectTransform holder) &&
            MarbleManager.instance.TryGetHolderIndex(holder, out int holderIndex) &&
            holderIndex == marble.index)
        {
            LoadMarble();
        }

        Vector2 inputPos = InputManager.instance.TouchWorldPos + randomOffset * offsetFactor;
        Vector3 target = new Vector3(inputPos.x, inputPos.y, -2);
        float d2Target = Vector3.Distance(target, marble.trans.position);
        float stepDistance = Time.deltaTime * marbleSpeedOnDrag * Mathf.Max(d2Target, accelerationTreshold);
        draggedMarblePreviousPos = marble.trans.position;

        if (d2Target <= stepDistance)
        {
            marble.transform.position = target;
        }
        else
        {
            marble.trans.position = marble.trans.position + (target - marble.trans.position).normalized * stepDistance;
        }

        return this;
    }

    private void LoadMarble()
    {
        int previousWhole = Mathf.FloorToInt(marble.currentLoadValue);

        if (marble.currentLoadValue < 2)
            marble.currentLoadValue += Time.deltaTime / marbleLoadingTime;
        else
            marble.currentLoadValue = 2;

        int flooredValue = Mathf.FloorToInt(marble.currentLoadValue);

        if (previousWhole != flooredValue)
        {
            //New loading level 
            marble.OnLevelUpdate(flooredValue);
            OnMarbleLevelUpdate?.Invoke(marble, flooredValue);
        }
    }
}
