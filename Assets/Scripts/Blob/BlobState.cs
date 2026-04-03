using UnityEngine;

public class BlobState : MonoBehaviour
{
    [SerializeField] BlobRenderer blobRend;

    private void Awake()
    {
        Channel.OnChannelUpdate += OnMarbleAbsorbed;
        Channel.OnChannelClear += OnMarbleRemoved;
    }
    private void OnDestroy()
    {
        Channel.OnChannelUpdate -= OnMarbleAbsorbed;
        Channel.OnChannelClear -= OnMarbleRemoved;
    }

    private void OnMarbleRemoved(int id)
    {
        //Maybe we should use a empty emotion value ...
        blobRend.SetMoodState(Mood.Fear, 0, id);
        blobRend.StartLerping();
    }

    private void OnMarbleAbsorbed(int slot, Mood marbleMood, float intensity)
    {
        blobRend.SetMoodState(marbleMood, intensity * 100, slot);
        blobRend.StartLerping();
    }
}
