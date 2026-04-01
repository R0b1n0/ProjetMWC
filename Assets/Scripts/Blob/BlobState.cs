using UnityEngine;

public class BlobState : MonoBehaviour
{
    [SerializeField] BlobRenderer blobRend;

    private void Awake()
    {
        Channel.OnChannelUpdate += OnMarbleAbsorbed;
    }
    private void OnDestroy()
    {
        Channel.OnChannelUpdate -= OnMarbleAbsorbed;
    }

    private void OnMarbleAbsorbed(int slot, Mood marbleMood, float intensity)
    {
        blobRend.SetMoodState(marbleMood, intensity * 100, slot);
        blobRend.StartLerping();
    }
}
