using System;
using UnityEngine;

public class MarbleInputs : MonoBehaviour
{
    [SerializeField] LayerMask marbleMask;
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
        if (OnDragBegin != null && TryCatchMarble(out MarbleData marble))
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
        Ray ray = new Ray(new Vector3(touchWorldPos.x, touchWorldPos.y, 0) - new  Vector3(0,0,2), Vector3.forward);

        RaycastHit hit;
        

        if (Physics.Raycast(new Vector3(touchWorldPos.x, touchWorldPos.y, 0) - new Vector3(0, 0, 2), Vector3.forward, out hit, marbleMask) && 
            hit.transform.TryGetComponent<MarbleData>(out MarbleData marbleHit))
        {
            marlbe = marbleHit;
            return true;
        }

        
        return false;
    }
}
