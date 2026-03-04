using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

[RequireComponent(typeof(RectTransform))]
public class SlotDrawer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject satelliteGo;
    [SerializeField] ArkDrawer outerDrawer;
    [SerializeField] ArkDrawer innerDrawer;

    [Header("Parameters")]
    [SerializeField] float outerCircleRotationSpeed;
    [SerializeField, Range(0,1)] float innerCircleRadius;
    [SerializeField, Range(0, 1)] float outerCircleRadius;
    [Min(0),SerializeField] int satelliteCount;
    [SerializeField] float satelliteMaxScale;
    [SerializeField] float salettilesSpeed;

    List<SatelliteBehaviour> satellites = new List<SatelliteBehaviour>();

    [SerializeField] float outerCircleAngleOffset;
    RectTransform holder;
    CircleCollider2D coll;

    private void Awake()
    {
        holder = GetComponent<RectTransform>();
        coll = GetComponent<CircleCollider2D>();
        
        //Create the satellites
        for (int i = 0; i < satelliteCount; i++)
        {
            GameObject satellite = Instantiate(satelliteGo);
            satellite.transform.parent = transform;
            satellites.Add(satellite.GetComponent<SatelliteBehaviour>());
            satellites[i].targetScale = satelliteMaxScale;
        }

        Utils.OnScreenRescale += Rescale;

        RotateSatellites();
        SetSatelliteCount(1);
    }

    private void OnDestroy()
    {
        Utils.OnScreenRescale -= Rescale;
    }

    private void Update()
    {
        //spin the outer circles
        outerCircleAngleOffset += Time.deltaTime * outerCircleRotationSpeed * Mathf.Deg2Rad;
        outerDrawer.SetOffset(outerCircleAngleOffset);

        RotateSatellites();
    }

    private void Rescale()
    {
        Vector3[] holderCornersS = new Vector3[4];
        holder.GetLocalCorners(holderCornersS);
        float holderHalfWidth = (holderCornersS[2].x - holderCornersS[0].x) / 2 * Utils.screen2World;

        outerDrawer.radius = holderHalfWidth * outerCircleRadius;
        innerDrawer.radius = holderHalfWidth * innerCircleRadius;
        coll.radius = (holderCornersS[2].x - holderCornersS[0].x) / 2 * innerCircleRadius;
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
                    0).normalized * innerDrawer.radius;
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
