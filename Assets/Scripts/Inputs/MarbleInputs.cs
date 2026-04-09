using System;
using UnityEngine;

public class MarbleInputs : MonoBehaviour
{
    [SerializeField] LayerMask marbleMask;
    [SerializeField] TagHandle marbleHolderTag;
    private MarbleData heldMarble;

    public static event Action<MarbleData> OnDragBegin;
    public static event Action<MarbleData> OnDragEnd;

    private void Start()
    {
        InputManager.instance.OnTouchStartEvent += OnTouchStarted;
        InputManager.instance.OnTouchEndEvent += OnTouchEnd;
    }

    private void OnTouchStarted()
    {
        if (OnDragBegin != null && TryCatchMarble(out MarbleData marble) && GameState.State == EGameState.game)
        {
            heldMarble = marble;
            OnDragBegin?.Invoke(heldMarble);
        }
    }
    private void OnTouchEnd()
    {
        if (OnDragEnd != null &&  heldMarble)
        {
            OnDragEnd?.Invoke(heldMarble);
            heldMarble = null;
        }
    }

    private bool TryCatchMarble(out MarbleData marlbe)
    {
        marlbe = null;
        Vector2 touchWorldPos = InputManager.instance.TouchWorldPos;

        RaycastHit2D hit = Physics2D.Raycast(new Vector3(touchWorldPos.x, touchWorldPos.y, 0), Vector2.zero, 0.1f);

        if (hit.transform != null && hit.transform.TryGetComponent(out SlotDrawer slot))
        {
            marlbe = MarbleManager.instance.GetMarble(slot.id);
            return true;
        }

        
        return false;
    }
}
