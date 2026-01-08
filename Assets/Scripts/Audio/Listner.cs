using AK.Wwise;
using UnityEngine;

public class Listner : MonoBehaviour
{
    float intensity;
    AkQueryRTPCValue type = AkQueryRTPCValue.RTPCValue_GameObject;
    [SerializeField] AK.Wwise.RTPC rTPC;

    // Update is called once per frame
    void Update()
    {
        Debug.Log(rTPC.GetGlobalValue());
    }
}
