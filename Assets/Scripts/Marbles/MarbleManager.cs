using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarbleManager : MonoBehaviour
{
    [SerializeField] Camera mainCam;
    [SerializeField] GameObject marblePb;
    [SerializeField] RectTransform[] holders;
    List<Marble> marbles = new List<Marble>();

    [SerializeField] AnimationCurve lerpInCurve;
    [SerializeField] float lerpInSpeed;

    [SerializeField] float idleSpeed;
    [SerializeField] float idleRange;

    [SerializeField] EmotionParameters moodDic;

    [SerializeField]
    [HideInInspector]
    private List<Mood> moodOrder;

    private void Start()
    {
        StartCoroutine(SetupOnStart());
    }

    private void Update()
    {
        AnimateIdle();
    }
    private void AnimateIdle()
    {
        for (int i = 0; i < marbles.Count; i++)
        {
            if (marbles[i].idling)
            {
                Marble marble = marbles[i];
                marble.trans.position = holders[marble.index].position + new Vector3(0,Mathf.Sin(marble.idleValue) * idleRange, 0);
                marble.idleValue += Time.deltaTime * idleSpeed;
            }
        }
    }

    public void TriggerAnimation()
    {
        StartCoroutine(LerpIn(marbles));
    }

    private IEnumerator SetupOnStart()
    {
        yield return new WaitForEndOfFrame();
        //Instantiate the marbles 
        for (int i = 0; i < holders.Length; i++)
        {
            GameObject marble = Instantiate(marblePb);
            marbles.Add(new Marble(moodDic.GetMoodInfo(moodOrder[i]).marbleColor, i, marble.transform));

            Vector3[] corners = new Vector3[4];
            holders[i].GetWorldCorners(corners);

            marble.transform.position = holders[i].position + new Vector3(0, -(corners[1].y - corners[0].y),0);
        }

        for (int i = 0; i < holders.Length; i++)
        {
            StartCoroutine(LerpIn(new List<Marble> { marbles[i] }));
            yield return new WaitForSeconds(0.3f);
        }
        
    }

    private IEnumerator LerpIn(List<Marble> marbles)
    {
        float t = 0;

        Vector3[] start = new Vector3[marbles.Count];
        Vector3[] destination = new Vector3[marbles.Count];
        Vector3[] corners = new Vector3[4];

        for (int i = 0;i < marbles.Count;i++)
        {
            RectTransform holder = holders[marbles[i].index];
            holder.GetWorldCorners(corners);
            start[i] = holder.position + new Vector3(0, -(corners[1].y - corners[0].y), 0);
            destination[i] = holder.position;
            marbles[i].idling = false;
        }

        while(t<=1)
        {
            for (int i = 0; i < marbles.Count; i++)
            {
                marbles[i].trans.position = start[i] + (destination[i] - start[i]) * lerpInCurve.Evaluate(t);
            }
            t += Time.deltaTime / 3;
            yield return null;
        }

        for (int i = 0; i < marbles.Count; i++)
        {
            marbles[i].trans.position = destination[i];
            marbles[i].idleValue = 0;
            marbles[i].idling = true;
        }
    }
}

public class Marble
{
    public Collider collider;
    public Transform trans;
    public Color color;
    public int index;
    public bool idling = false;
    public float idleValue = 0;
    public Material mat;

    public Marble(Color color, int index, Transform transform)
    {
        trans = transform;
        collider = trans.GetComponent<Collider>();        
        this.color = color;
        this.index = index;
        Renderer renderer = trans.GetComponent<Renderer>();
        mat = new (renderer.material);
        mat.color = color;
        renderer.material = mat;
    }
}