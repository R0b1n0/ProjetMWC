using System.Collections.Generic;
using UnityEngine;

public class SlotDrawer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject satelliteGo;
    [SerializeField] ArkDrawer outerDrawer;
    [SerializeField] ArkDrawer innerDrawer;

    [Header("Parameters")]
    [SerializeField] float outerCircleRotationSpeed;
    [SerializeField] float innerCircleRadius;
    [SerializeField] float outerCircleRadius;
    [Min(0),SerializeField] int satelliteCount;
    [SerializeField] float satelliteMaxScale;

    List<SatelliteBehaviour> satellites = new List<SatelliteBehaviour>();

    float outerCircleAngleOffset;

    private void Awake()
    {
        innerDrawer.radius = innerCircleRadius;

        //Create the satellites
        for (int i = 0; i < satelliteCount; i++)
        {
            GameObject satellite = Instantiate(satelliteGo);
            satellite.transform.parent = transform;
            satellites.Add(satellite.GetComponent<SatelliteBehaviour>());
            satellites[i].targetScale = satelliteMaxScale;
        }

        outerDrawer.radius = outerCircleRadius;
        innerDrawer.radius = innerCircleRadius;

        RotateSatellites();
        SetSatelliteCount(1);
    }

    private void Update()
    {

        //spin the outer circles
        outerCircleAngleOffset += Time.deltaTime * outerCircleRotationSpeed * Mathf.Deg2Rad;
        outerDrawer.SetOffset(outerCircleAngleOffset);

        RotateSatellites();
    }

    private void RotateSatellites()
    {
        for (int i = 0;i < satellites.Count; i++)
        {
            float offset = i * Mathf.PI * 2 / satellites.Count;

            satellites[i].Pos = transform.position + 
                new Vector3(
                    Mathf.Cos(outerCircleAngleOffset + offset), 
                    Mathf.Sin(outerCircleAngleOffset + offset), 
                    0).normalized * innerCircleRadius ;
        }
    }

    public void SetSatelliteCount(int count)
    {
        for (int i = 0; i < satelliteCount; i++)
        {
            if (i < count )
                satellites[i].ScaleUp();
            else 
                satellites[i].ScaleDown();
        }
    }

}
