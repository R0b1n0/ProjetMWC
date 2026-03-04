using UnityEngine;

public class BlobState : MonoBehaviour
{
    [SerializeField] BlobManager blobRend;

    private void Awake()
    {
        AbsorbState.OnMarbleAbsorbtion += OnMarbleAbsorbed;
    }
    private void OnDestroy()
    {
        AbsorbState.OnMarbleAbsorbtion -= OnMarbleAbsorbed;
    }

    private void OnMarbleAbsorbed(Mood marbleMood, float intensity)
    {
        blobRend.SetMoodState(marbleMood, intensity * 100, 0);
        blobRend.StartLerping();
    }
}
