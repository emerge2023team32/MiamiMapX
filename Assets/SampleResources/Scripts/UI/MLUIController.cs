/*===============================================================================
Copyright (c) 2023 PTC Inc. and/or Its Subsidiary Companies. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
#if ML_ASSETS_IMPORTED
using UnityEngine.XR.MagicLeap;
#endif

public class MLUIController : MonoBehaviour
{
    public MLCanvas mainCanvas;
    public MLCanvas homeCanvas;
    public MLCanvas bumperCanvas;

    ML6DOFController controller;

    public void Awake()
    {
        controller = FindObjectOfType<ML6DOFController>();
    }

    public void Start()
    {
#if ML_ASSETS_IMPORTED
#if UNITY_MAGICLEAP || UNITY_ANDROID
        controller.BumperDown += OnBumperDown;
        controller.MenuDown += OnMenuDown;
#endif
#endif
    }

    public void Stop()
    {
#if ML_ASSETS_IMPORTED
#if UNITY_MAGICLEAP || UNITY_ANDROID
        controller.BumperDown -= OnBumperDown;
        controller.MenuDown -= OnMenuDown;
#endif
#endif
    }

    public void GoToMain()
    {
        if (mainCanvas != null)
            mainCanvas.SetInteractionEnabled(true);

        if (homeCanvas != null)
            homeCanvas.gameObject.SetActive(false);

        if (bumperCanvas != null)
            bumperCanvas.gameObject.SetActive(false);
    }

    public void GoToHome()
    {
        if (homeCanvas != null)
        {
            if (bumperCanvas != null)
                bumperCanvas.gameObject.SetActive(false);

            homeCanvas.gameObject.SetActive(true);

            if (mainCanvas != null)
                mainCanvas.SetInteractionEnabled(false);
        }
    }

    public void GoToBumper()
    {
        if (bumperCanvas != null)
        {
            if (homeCanvas != null)
                homeCanvas.gameObject.SetActive(false);

            bumperCanvas.gameObject.SetActive(true);
            
            if (mainCanvas != null)
                mainCanvas.SetInteractionEnabled(false);
        }
    }

#if ML_ASSETS_IMPORTED
    void OnBumperDown()
    {
        if (bumperCanvasVisible)
        {
            GoToMain();
        }
        else
        {
            GoToBumper();
        }
    }
    
    void OnMenuDown()
    {
        if (homeCanvasVisible)
        {
            GoToMain();
        }
        else
        {
            GoToHome();
        }
    }
    
#endif
    
    bool homeCanvasVisible => homeCanvas != null && homeCanvas.gameObject.activeSelf;

    bool bumperCanvasVisible => bumperCanvas != null && bumperCanvas.gameObject.activeSelf;
}
