using UnityEngine;

public class MarbleBehaviour : MonoBehaviour
{
    private Collider coll;
    public Transform trans;
    public Color color;
    public int index;
    public MarbleState state {  get; private set; }
    public float lerpValue = 0;
    public Material mat;

    public Vector3 OnReleasePos { get; private set; }

    Renderer rend;


    private void Awake()
    {
        coll = GetComponent<Collider>();
        trans = transform;
        rend = trans.GetComponent<Renderer>();
        mat = new(rend.material);
        rend.material = mat;
    }

    public void Initialize(Color color, int index)
    {
        this.color = color;
        this.index = index;
        mat.color = color;
        SetState(MarbleState.lerpIn);
    }

    public void SetState(MarbleState newState)
    {
        //Enter
        switch (newState)
        {
            case MarbleState.lerpIn:
                coll.enabled = false;
                break;
            case MarbleState.dragged:
                coll.enabled = false; 
                break;
            case MarbleState.idle:
                coll.enabled = true;
                break;
            case MarbleState.recover: 
                coll.enabled = false;
                break;
            case MarbleState.consummed:
                coll.enabled = false;
            break;
        }

        state = newState;
    }

    public void OnGrabbed()
    {
        SetState(MarbleState.dragged);

    }

    public void OnRelease()
    {
        OnReleasePos = trans.position;
        lerpValue = 0;
    }
}

public enum MarbleState
{
    dragged,
    idle, 
    recover, 
    consummed, 
    lerpIn
}