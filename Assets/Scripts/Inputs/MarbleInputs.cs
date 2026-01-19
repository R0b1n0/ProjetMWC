using System;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class MarbleInputs : MonoBehaviour
{
    [SerializeField] LayerMask marbleMask;
    private MarbleBehaviour heldMarble;

    public static Action<MarbleBehaviour> OnDragBegin;
    public static Action<MarbleBehaviour> OnDragEnd;

    Vector3 og; 

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
            heldMarble.OnGrabbed();
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
            heldMarble.OnRelease();
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

    private void OnDrawGizmos()
    {
        
        Gizmos.DrawRay(new Ray(og, Vector3.forward));
    }
}
