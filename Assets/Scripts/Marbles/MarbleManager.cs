using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarbleManager : MonoBehaviour
{
    public static MarbleManager instance;

    [SerializeField] Camera mainCam;
    [SerializeField] GameObject marblePb;
    [SerializeField] RectTransform[] holders;
    [SerializeField] string marbleHolderTag;

    List<MarbleData> marbles = new List<MarbleData>();
    List<SlotDrawer> drawers = new List<SlotDrawer>();

    [Header("Params")]
    [SerializeField, Range(0, 1)] float marbleScaleRelativToHolder;

    [SerializeField]
    [HideInInspector]
    private List<Mood> moodOrder;

    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);

        StartCoroutine(SetupOnStart());

        Utils.OnScreenRescale += UpdateMarblesOnCanvaResize;
        DraggingState.OnMarbleLevelUpdate += OnIntensityLevelUpdate;
    }

    private void OnDestroy()
    {
        Utils.OnScreenRescale -= UpdateMarblesOnCanvaResize;
        DraggingState.OnMarbleLevelUpdate += OnIntensityLevelUpdate;
    }

    private void UpdateMarblesOnCanvaResize()
    {
        Vector3[] corners = new Vector3[4];
        holders[0].GetWorldCorners(corners);
        float scale = (corners[3].x - corners[0].x) * marbleScaleRelativToHolder;

        for (int i = 0; i < marbles.Count; i++)
        {
            marbles[i].UpdateOnCanvaRescale(scale);
        }
    }
    #region Utils
    public MoodProperties GetMoodData(int index)
    {
        return EmotionParameters.Instance.GetMoodInfo(moodOrder[index]);
    }
    public Vector3 GetSlotPos(int slotIndex)
    {
        Vector3[] corners = new Vector3[4];
        RectTransform holder = holders[slotIndex];
        holder.GetWorldCorners(corners);
        return new Vector3(holder.position.x, holder.position.y, 0);
    }
    public MarbleData GetMarble(int index)
    { 
        return marbles[index];
    }
    public Vector3 GetLerpInStartPos(int slotIndex)
    {
        Vector3[] corners = new Vector3[4];
        RectTransform holder = holders[slotIndex];
        holder.GetWorldCorners(corners);
        return new Vector3(holder.position.x, holder.position.y - (corners[1].y - corners[0].y), 0);
    }
    #endregion
    private void OnIntensityLevelUpdate(MarbleData marble, int newLevel)
    {
        //Maybe this sould be in the holder drawer class
        drawers[marble.index].SetSatelliteCount(newLevel+1);
    }
    private IEnumerator SetupOnStart()
    {
        yield return new WaitForEndOfFrame();

        Vector3[] corners = new Vector3[4];
        holders[0].GetWorldCorners(corners);
        float scale = (corners[3].x - corners[0].x) * marbleScaleRelativToHolder;

        //Instantiate the marbles and register slots 
        for (int i = 0; i < holders.Length; i++)
        {
            GameObject marble = Instantiate(marblePb,new Vector3(500,0,0), Quaternion.identity);
            MarbleData marbleBh = marble.GetComponent<MarbleData>();
            marbles.Add(marbleBh);
            marbleBh.Initialize(moodOrder[i], EmotionParameters.Instance.GetMoodInfo(moodOrder[i]).marbleColor, i, scale);
            drawers.Add(holders[i].GetComponent<SlotDrawer>());
            drawers[drawers.Count - 1].id = i;

            yield return new WaitForSeconds(0.3f);
        }
    }
}