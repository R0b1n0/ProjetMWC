using UnityEngine;

public class ChannelInput : MonoBehaviour
{
    [SerializeField] Channel channelManager;
    [SerializeField] float doubleTapGap;

    float dtSinceSelect;
    ChannelDisplay selectedSlot;

    private void Start()
    {
        InputManager.instance.OnTouchStartEvent += OnTouchStarted;
    }
    private void OnDestroy()
    {
        InputManager.instance.OnTouchStartEvent -= OnTouchStarted;
    }

    private void OnTouchStarted()
    {
        if (selectedSlot)
            dtSinceSelect += Time.deltaTime;

        if (TryCatchChannel(out ChannelDisplay channel))
        {
            if (channel != selectedSlot)
            {
                //Select new channel
                channelManager.SetCurrentChannel(channel);
                dtSinceSelect = 0;
            }
            else if (dtSinceSelect > doubleTapGap)
            {
                //That's a double click, empty the slot 
            }
        }
    }

    private bool TryCatchChannel(out ChannelDisplay channel)
    {
        channel = null;
        Vector2 touchWorldPos = InputManager.instance.TouchWorldPos;

        RaycastHit2D hit = Physics2D.Raycast(new Vector3(touchWorldPos.x, touchWorldPos.y, 0), Vector2.zero, 0.1f);

        if (hit.transform != null && hit.transform.TryGetComponent(out ChannelDisplay slot))
        {
            channel = slot;
            return true;
        }


        return false;
    }
}
