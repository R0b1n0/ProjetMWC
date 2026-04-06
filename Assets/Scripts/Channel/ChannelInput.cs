using System;
using UnityEngine;

public class ChannelInput : MonoBehaviour
{
    [SerializeField] Channel channelManager;
    [SerializeField] float doubleTapGap;

    public static event Action OnSlotSelect;
    public static event Action OnSlotClear;
    float dtSinceSelect;
    ChannelDisplay selectedChannel;

    int consecutivClick = 0;

    private void Start()
    {
        InputManager.instance.OnTouchStartEvent += OnTouchStarted;
    }
    private void OnDestroy()
    {
        InputManager.instance.OnTouchStartEvent -= OnTouchStarted;
    }

    private void Update()
    {
        if (selectedChannel)
        {
            dtSinceSelect += Time.deltaTime;

            if (dtSinceSelect > doubleTapGap)
            {
                consecutivClick = 0;
            }
        }
    }

    private void OnTouchStarted()
    {
        //Detect channel slot
        if (TryCatchChannel(out ChannelDisplay channel))
        {
            //Try to set the new channel 
            if (channelManager.TrySetCurrentChannel(channel.id))
            {
                OnSlotSelect?.Invoke();
                consecutivClick = 1;
            }
            else if (selectedChannel)
            {
                //Clicked on the same slot
                consecutivClick++;

                if (consecutivClick > 1)
                {
                    EnptySlot();
                    consecutivClick = 0;
                }
            }
            dtSinceSelect = 0;
            selectedChannel = channel;
        }
    }

    private void EnptySlot()
    {
        if (!selectedChannel.empty)
        {
            OnSlotClear?.Invoke();
            channelManager.EmptyCurrentChannel();
        }
    }

    private bool TryCatchChannel(out ChannelDisplay channel)
    {
        channel = null;
        if (GameState.State != EGameState.game)
            return false;

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
