/*===============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using Vuforia;

public class ModelTargetsHideGuideView : MonoBehaviour
{
    ModelTargetBehaviour[] mModelTargetBehaviours;

    public void Awake()
    {
        mModelTargetBehaviours = FindObjectsOfType<ModelTargetBehaviour>();
    }

    public void OnEnable()
    {
        // Disable the GuideView for all model targets
        foreach (var modelTargetBehaviour in mModelTargetBehaviours)
            modelTargetBehaviour.GuideViewMode = ModelTargetBehaviour.GuideViewDisplayMode.NoGuideView;
    }

    public void OnDisable()
    {
        // Enable the GuideView for all model targets
        foreach (var modelTargetBehaviour in mModelTargetBehaviours)
            modelTargetBehaviour.GuideViewMode = ModelTargetBehaviour.GuideViewDisplayMode.GuideView3D;
    }
}
