using System;
using UnityEngine;

public class MarbleInputs : MonoBehaviour
{
    [SerializeField] LayerMask marbleMask;
    private MarbleBehaviour heldMarble;

    public static event Action<MarbleBehaviour> OnDragBegin;
    public static event Action<MarbleBehaviour> OnDragEnd;

    private void Start()
    {
        InputManager.instance.OnTouchStartEvent += OnTouchStarted;
        InputManager.instance.OnTouchEndEvent += OnTouchEnd;
    }

    private void OnTouchStarted()
    {
        if (TryCatchMarble(out MarbleBehaviour marble))
        {
            heldMarble = marble;
            if (OnDragBegin != null)
            {
                OnDragBegin.Invoke(heldMarble);
            }
        }
    }
    private void OnTouchEnd()
    {
        if (heldMarble)
        {
            if (OnDragEnd != null)
            {
                OnDragEnd.Invoke(heldMarble);
            }
            heldMarble = null;
        }
    }

    private bool TryCatchMarble(out MarbleBehaviour marlbe)
    {
        marlbe = null;

        Vector2 touchWorldPos = InputManager.instance.TouchWorldPos;
        Ray ray = new Ray(new Vector3(touchWorldPos.x, touchWorldPos.y, 0) - new  Vector3(0,0,2), Vector3.forward);

        RaycastHit hit;
        

        if (Physics.Raycast(new Vector3(touchWorldPos.x, touchWorldPos.y, 0) - new Vector3(0, 0, 2), Vector3.forward, out hit, marbleMask) && 
            hit.transform.TryGetComponent<MarbleBehaviour>(out MarbleBehaviour marbleHit))
        {
            marlbe = marbleHit;
            return true;
        }

        
        return false;
    }
}
