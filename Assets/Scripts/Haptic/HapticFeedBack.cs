using UnityEngine;
using static Solo.MOST_IN_ONE.MOST_HapticFeedback;

public class HapticFeedBack : MonoBehaviour
{
    [SerializeField] CustomHapticPattern marbleLvl2;
    [SerializeField] CustomHapticPattern marbleLvl3;

    private void Awake()
    {
        DraggingState.OnMarbleLevelUpdate += OnMarbleLevelUpdate;
    }

    private void OnDestroy()
    {
        DraggingState.OnMarbleLevelUpdate -= OnMarbleLevelUpdate;
    }

    private void OnMarbleLevelUpdate(MarbleData data, int arg2)
    {
        if (arg2 == 1)
        {
            GeneratePattern(marbleLvl2);
        }
        else if (arg2 == 2)
        {
            GeneratePattern(marbleLvl2);
        }
    }
}
