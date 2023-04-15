/*===============================================================================
Copyright (c) 2023 PTC Inc. and/or Its Subsidiary Companies. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using System;
using UnityEngine;
using UnityEngine.Events;
#if ML_ASSETS_IMPORTED
using UnityEngine.XR.MagicLeap;
#if UNITY_MAGICLEAP || UNITY_ANDROID
using UnityEngine.InputSystem;
using UnityEngine.XR.InteractionSubsystems;
#endif
#endif

[System.Serializable]
public class FloatEvent : UnityEvent<float> { }

public class ML6DOFController : MonoBehaviour
{
#if ML_ASSETS_IMPORTED
#if UNITY_MAGICLEAP || UNITY_ANDROID
    public event Action BumperDown;
    public event Action MenuDown;
    public event Action<GestureSubsystem.Extensions.TouchpadGestureEvent> TouchpadGestureCompleted;
    public event Action<GestureSubsystem.Extensions.TouchpadGestureEvent> TouchpadGestureUpdated;
    public event Action TriggerUp;
    
    MagicLeapInputs mMagicLeapInputs;
    MagicLeapInputs.ControllerActions mControllerActions;
    GestureSubsystemComponent mGestureSubsystemComponent;

    bool mTriggered;
#endif
#endif

#if ML_ASSETS_IMPORTED
#if UNITY_MAGICLEAP || UNITY_ANDROID
    void Start()
    {
        mMagicLeapInputs = new MagicLeapInputs();
        mMagicLeapInputs.Enable();
        mControllerActions = new MagicLeapInputs.ControllerActions(mMagicLeapInputs);
        
        mControllerActions.Bumper.performed += HandleOnBumper;
        mControllerActions.Menu.performed += HandleOnMenu;
        mControllerActions.Trigger.performed += HandleOnTrigger;
        mControllerActions.Trigger.canceled += HandleOnTriggerCanceled;
        
        MLDevice.RegisterGestureSubsystem();
        MLDevice.GestureSubsystemComponent.onTouchpadGestureChanged += HandleTouchpadGestureChange;
    }

    void HandleOnBumper(InputAction.CallbackContext obj)
    {
        var isBumperDown = obj.ReadValueAsButton();
        if (isBumperDown)
            BumperDown?.Invoke();
    }
    
    void HandleOnMenu(InputAction.CallbackContext obj)
    {
        var isMenuDown = obj.ReadValueAsButton();
        if (isMenuDown)
            MenuDown?.Invoke();
    }
    
    void HandleOnTrigger(InputAction.CallbackContext obj)
    {
        var isTriggerDown = obj.ReadValueAsButton();
        var triggerValue = obj.ReadValue<float>();

        if (isTriggerDown && triggerValue > 0.5f)
            mTriggered = true;
    }
    
    void HandleOnTriggerCanceled(InputAction.CallbackContext obj)
    {
        if (mTriggered)
            TriggerUp?.Invoke();
        
        mTriggered = false;
    }
    
    void HandleTouchpadGestureChange(GestureSubsystem.Extensions.TouchpadGestureEvent touchpadGestureEvent)
    {
        if (touchpadGestureEvent.state == GestureState.Updated)
            TouchpadGestureUpdated?.Invoke(touchpadGestureEvent);
        else if (touchpadGestureEvent.state == GestureState.Completed)
            TouchpadGestureCompleted?.Invoke(touchpadGestureEvent);
    }
    
    void Update()
    {
        if (mControllerActions.IsTracked.IsPressed())
        {
            transform.position = mControllerActions.Position.ReadValue<Vector3>();

            transform.rotation =mControllerActions.Rotation.ReadValue<Quaternion>();    
        }
    }

    public void Vibrate()
    {
        Handheld.Vibrate();
    }

    // Handles the disposing all of the input events.
    void OnDestroy()
    {
        mControllerActions.Bumper.performed -= HandleOnBumper;
        mControllerActions.Menu.performed -= HandleOnBumper;
        mControllerActions.Trigger.performed -= HandleOnTrigger;
        mControllerActions.Trigger.canceled -= HandleOnTriggerCanceled;
        
        MLDevice.GestureSubsystemComponent.onTouchpadGestureChanged -= HandleTouchpadGestureChange;
        MLDevice.RegisterGestureSubsystem();
        
        mMagicLeapInputs.Dispose();
    }
#endif
#endif
}
