/*==============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
==============================================================================*/

using UnityEngine;
using Vuforia;

public class ModelTargetsManager : MonoBehaviour
{
    enum TargetMode
    {
        STANDARD,
        ADVANCED
    }

    [Header("Initial Model Target Mode")]
    [SerializeField] TargetMode ModelTargetMode = TargetMode.STANDARD;

    [Header("Model Target Behaviours")] 
    [SerializeField] ModelTargetBehaviour StandardTarget = null;
    [SerializeField] ModelTargetBehaviour[] AdvancedTargets = null;

    [Header("UI Images")]
    [SerializeField] GameObject ImageStandard = null;
    [SerializeField] GameObject ImageAdvanced = null;

    void Start()
    {
        VuforiaApplication.Instance.OnVuforiaStarted += VuforiaStarted;
    }

    void OnDestroy()
    { 
        VuforiaApplication.Instance.OnVuforiaStarted -= VuforiaStarted;
    }

    void VuforiaStarted()
    {
        // We can only have one ModelTarget DataSet active at a time, so disable all MTBs at start.
        var behaviours = FindObjectsOfType<ModelTargetBehaviour>();
        foreach (var behaviour in behaviours)
            behaviour.enabled = false;

        switch (ModelTargetMode)
        {
            case TargetMode.STANDARD:
                // Start with the Standard Model Target
                SelectStandardModelTarget();
                break;
            case TargetMode.ADVANCED:
                // Start with the Advanced Model Targets
                SelectAdvancedModelTargets();
                break;
        }
    }

    public void SelectStandardModelTarget()
    {
        foreach (var target in AdvancedTargets)
            target.enabled = false;
        StandardTarget.enabled = true;
        
        ModelTargetMode = TargetMode.STANDARD;

        ImageStandard.SetActive(true);
        ImageAdvanced.SetActive(false);
    }

    public void SelectAdvancedModelTargets()
    {
        StandardTarget.enabled = false;
        foreach (var target in AdvancedTargets)
            target.enabled = true;
        
        ModelTargetMode = TargetMode.ADVANCED;

        ImageStandard.SetActive(false);
        ImageAdvanced.SetActive(true);
    }
}