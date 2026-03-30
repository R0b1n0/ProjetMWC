using System.Collections.Generic;
using UnityEngine;

public class BlobPartMovement : MonoBehaviour
{
    [SerializeField][Range(0f, 1f)] float movementType;
    [SerializeField] AnimationCurve speedCurve;
    [SerializeField] public float movementAreaRadius;
    [SerializeField] public float dispertionFactor;
    [SerializeField] float dispertionMax;
    [SerializeField] public float speedFactor;
    [SerializeField] AnimationCurve speedFactorCurve;
    [SerializeField] float lowSpeedValue;
    [SerializeField] float highSpeedValue;
    [SerializeField] float maxShakeRange;
    [SerializeField, Range(0, 1)] public float shakeFactor;

    public void UpdatePartPos(List<Part> parts)
    {
        Vector2 computePos;
        //Compute shakeFactor
        Vector2 shakeOffset = new Vector2(UnityEngine.Random.Range(-1, 1), UnityEngine.Random.Range(-1, 1)).normalized * maxShakeRange;
        shakeOffset *= shakeFactor;

        foreach (Part part in parts)
        {
            //Linear lerp constant speed
            float newSpeedFactor = lowSpeedValue + (speedFactorCurve.Evaluate(speedFactor) * highSpeedValue);
            part.lerpPhase += Time.deltaTime * (part.lerpSpeed / (Vector2.Distance(part.origin, part.destination) * 2)) * newSpeedFactor;

            float lerpValue = speedCurve.Evaluate(part.lerpPhase);
            //Linear move 
            Vector2 linearLerp = Vector2.Lerp(part.origin, part.destination, lerpValue);

            //Curved movement 
            Vector2 circularLerp = Vector3.Slerp(part.origin, part.destination, lerpValue);

            computePos = Vector2.Lerp(linearLerp, circularLerp, movementType);

            if (part.lerpPhase >= 1)
            {
                //Circle finished lerping
                part.origin = part.destination;
                float movementAreaRad = movementAreaRadius + dispertionFactor * dispertionMax;
                part.destination.Set(UnityEngine.Random.Range(-movementAreaRad, movementAreaRad), UnityEngine.Random.Range(-movementAreaRad, movementAreaRad));
                part.lerpPhase = 0;
            }

            part.currentPos = computePos + shakeOffset;
        }
    }
}
