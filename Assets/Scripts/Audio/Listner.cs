using UnityEngine;

public class Listner : MonoBehaviour
{
    private void Awake()
    {
        AkUnitySoundEngine.LoadBank("Init", out _);
        AkUnitySoundEngine.LoadBank("soundbankMWC", out _);
    }
}
