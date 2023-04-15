/*===============================================================================
Copyright (c) 2023 PTC Inc. and/or Its Subsidiary Companies. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
#if ML_ASSETS_IMPORTED
using UnityEngine.XR.MagicLeap;
#endif

public class MLCanvas : MonoBehaviour
{
    public int canvasPlane = 0;

    public enum HeadTracking {
        None, Fixed, Smooth
    }
    public HeadTracking headTracking;
    public float headTrackingSpeed;

    public Camera mainCamera;

    List<MLButton> buttons;
    MLButton selectedButton = null;

    ML6DOFController controller;

    AnimatedFloat animatedDepthPlane;
    AnimatedFloat animatedAlpha;

    Renderer[] renderers;
    Image[] images;
    Text[] texts;
    
    public void Awake()
    {
        animatedDepthPlane = new AnimatedFloat(canvasPlane);
        animatedAlpha = new AnimatedFloat(1.0f);

        buttons = new List<MLButton>(GetComponentsInChildren<MLButton>());
        controller = FindObjectOfType<ML6DOFController>();

        renderers = GetComponentsInChildren<Renderer>(true);
        images = GetComponentsInChildren<Image>(true);
        texts = GetComponentsInChildren<Text>(true);
    }

    public void OnEnable()
    {
#if ML_ASSETS_IMPORTED
#if UNITY_MAGICLEAP || UNITY_ANDROID
        controller.TouchpadGestureCompleted += OnTouchpadGestureCompleted;
        controller.TouchpadGestureUpdated += OnTouchpadGestureUpdated;
        controller.TriggerUp += OnTriggerUp;
#endif
        
        if ((headTracking == HeadTracking.Fixed) || (headTracking == HeadTracking.Smooth))
        {
            gameObject.transform.position = GetHeadPosition();
            gameObject.transform.rotation = GetHeadRotation();
        }
#endif
    }

    public void OnDisable()
    {
#if ML_ASSETS_IMPORTED
#if UNITY_MAGICLEAP || UNITY_ANDROID
        controller.TouchpadGestureCompleted -= OnTouchpadGestureCompleted;
        controller.TouchpadGestureUpdated -= OnTouchpadGestureUpdated;
        controller.TriggerUp -= OnTriggerUp;
#endif
        SelectButton(null);
#endif
    }

    public void Update()
    {
        if (headTracking == HeadTracking.Fixed)
        {
            gameObject.transform.position = GetHeadPosition();
            gameObject.transform.rotation = GetHeadRotation();
        }
        else if (headTracking == HeadTracking.Smooth)
        {
            var deltaSpeed = Mathf.Clamp01(Time.deltaTime * headTrackingSpeed);
            gameObject.transform.position = Vector3.SlerpUnclamped(gameObject.transform.position, GetHeadPosition(), deltaSpeed);
            gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, GetHeadRotation(), deltaSpeed);
        }

        if (interactionEnabled)
        {
            RaycastHit hit;
            MLButton hitButton = null;
            if (Physics.Raycast(controller.transform.position, controller.transform.forward, out hit))
                hitButton = hit.collider.GetComponentInParent<MLButton>();

            if (hitButton != null)
                SelectButton(hitButton);
        }

        if (animatedAlpha.IsAnimating())
            SetAlpha(animatedAlpha.GetValue());
    }

    Vector3 GetHeadPosition()
    {
        var distance = MLConstants.CANVAS_DEPTH - animatedDepthPlane.GetValue() * MLConstants.CANVAS_DEPTH_PLANE_STEP;
        return mainCamera.transform.position + (mainCamera.transform.forward * distance);
    }

    Quaternion GetHeadRotation()
    {
        return Quaternion.LookRotation(gameObject.transform.position - mainCamera.transform.position);
    }

    bool interactionEnabled = true;

    public void SetInteractionEnabled(bool value)
    {
        interactionEnabled = value;

        if (interactionEnabled)
        {
            animatedDepthPlane.AnimateToValue(canvasPlane, MLConstants.ANIMATION_TIME);
            animatedAlpha.AnimateToValue(1.0f, MLConstants.ANIMATION_TIME);

            SetCollidersEnabled(true);
        }
        else
        {
            animatedDepthPlane.AnimateToValue(canvasPlane - 1, MLConstants.ANIMATION_TIME);
            animatedAlpha.AnimateToValue(0.4f, MLConstants.ANIMATION_TIME);

            SetCollidersEnabled(false);

            SelectButton(null);
        }
    }

    void SetCollidersEnabled(bool value)
    {
        var colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders)
            c.enabled = value;
    }

    void SelectButton(MLButton button)
    {
#if ML_ASSETS_IMPORTED && (UNITY_MAGICLEAP || UNITY_ANDROID)
        if (button != selectedButton)
        {
            if (selectedButton != null)
            {
                selectedButton.SetSelected(false);
            }

            selectedButton = button;

            if (selectedButton != null)
            {
                selectedButton.SetSelected(true);
#if UNITY_MAGICLEAP || UNITY_ANDROID
                controller.Vibrate();
#endif
            }
        }
#endif
    }

    void PreviousButton()
    {
        if (buttons.Count == 0)
            return;
        
        if (selectedButton == null)
            SelectButton(buttons[0]);
        else
        {
            var currentIndex = buttons.IndexOf(selectedButton);
            var previousIndex = (buttons.Count + currentIndex - 1) % buttons.Count;
            SelectButton(buttons[previousIndex]);
        }
    }

    void NextButton()
    {
        if (buttons.Count == 0)
            return;

        if (selectedButton == null)
            SelectButton(buttons[0]);
        else
        {
            var currentIndex = buttons.IndexOf(selectedButton);
            var nextIndex = (currentIndex + 1) % buttons.Count;
            SelectButton(buttons[nextIndex]);
        }
    }

    void SetAlpha(float alpha)
    {
        foreach (var r in renderers)
        {
            foreach (var m in r.materials)
            {
                var color = m.color;
                color.a = alpha;
                m.color = color;
            }
        }

        foreach (var i in images)
        {
            var color = i.color;
            color.a = alpha;
            i.color = color;
        }

        foreach (var t in texts)
        {
            var color = t.color;
            color.a = alpha;
            t.color = color;
        }
    }

#if ML_ASSETS_IMPORTED
#if UNITY_MAGICLEAP || UNITY_ANDROID
    void OnTouchpadGestureCompleted(GestureSubsystem.Extensions.TouchpadGestureEvent touchpadGesture)
    {
        if (interactionEnabled)
        {
            OnControllerTouchpadGesture(touchpadGesture);
        }
    }

    void OnTouchpadGestureUpdated(GestureSubsystem.Extensions.TouchpadGestureEvent touchpadGesture)
    {
        if (interactionEnabled)
        {
            OnControllerTouchpadGesture(touchpadGesture);
        }
    }
    
    void OnControllerTouchpadGesture(GestureSubsystem.Extensions.TouchpadGestureEvent touchpadGesture)
    {
        switch (touchpadGesture.direction)
        {
            case InputSubsystem.Extensions.TouchpadGesture.Direction.Left:
            case InputSubsystem.Extensions.TouchpadGesture.Direction.Up:
            case InputSubsystem.Extensions.TouchpadGesture.Direction.CounterClockwise:
                PreviousButton();
                break;
            case InputSubsystem.Extensions.TouchpadGesture.Direction.Right:
            case InputSubsystem.Extensions.TouchpadGesture.Direction.Down:
            case InputSubsystem.Extensions.TouchpadGesture.Direction.Clockwise:
                NextButton();
                break;
        }
    }

    void OnTriggerUp()
    {
        if (selectedButton != null)
        {
            controller.Vibrate();
            selectedButton.onClick.Invoke();
        }
    }
#endif
#endif 
}
