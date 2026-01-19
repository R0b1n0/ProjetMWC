using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] Camera cam;

    public Action OnTouchStartEvent;
    public Action OnTouchEndEvent;

    public static InputManager instance;
    private TouchAction inputActions;

    public Vector2 TouchScreenPos => inputActions.Touch.TouchPos.ReadValue<Vector2>();
    public Vector2 TouchWorldPos => cam.ScreenToWorldPoint(TouchScreenPos);

    private void Awake()
    {
        inputActions = new();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        } 
            
    }
    private void Start()
    {
        inputActions.Touch.TouchHold.started += OnTouchStart;
        inputActions.Touch.TouchHold.canceled += OnTouchEnd;
    }
    private void OnEnable()
    {
        inputActions.Enable();
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void OnTouchStart(InputAction.CallbackContext ctxt)
    {
        if (OnTouchStartEvent != null)
            OnTouchStartEvent.Invoke();
    }
    private void OnTouchEnd(InputAction.CallbackContext ctxt)
    {
        if (OnTouchEndEvent != null)
            OnTouchEndEvent.Invoke();
    }
}
