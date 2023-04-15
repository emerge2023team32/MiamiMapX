/*===============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using Vuforia;

public class ModelTargetsHideInstructions : MonoBehaviour
{
    public GameObject Target;

    ModelTargetBehaviour[] mModelTargetBehaviours;

    bool mIsRenderingGuideView;
    bool mTargetsAreRendered;

    public void Awake()
    {
        mModelTargetBehaviours = FindObjectsOfType<ModelTargetBehaviour>();
    }

    public void Start()
    {
        // Listen for any status changes of the model targets
        foreach (var modelTargetBehaviour in mModelTargetBehaviours)
        {
            modelTargetBehaviour.OnTargetStatusChanged += TargetStatusChanged;
            modelTargetBehaviour.OnBehaviourDestroyed += BehaviourDestroyed;
        }
    }

    void BehaviourDestroyed(ObserverBehaviour behaviour)
    {
        behaviour.OnTargetStatusChanged -= TargetStatusChanged;
        behaviour.OnBehaviourDestroyed -= BehaviourDestroyed;
    }

    public void TargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        // If the guide view visibility or the rendering state changes, we update the visibility
        // of the instructions
        if (IsGuideViewRendered(status.StatusInfo) != mIsRenderingGuideView ||
            ShouldBeRendered(status.Status) != mTargetsAreRendered)
        {
            UpdateVisibility();
        }
    }

    bool IsGuideViewRendered(StatusInfo statusInfo)
    {
        if (statusInfo == StatusInfo.RECOMMENDING_GUIDANCE)
            return true;

        return false;
    }

    bool ShouldBeRendered(Status status)
    {
        return status == Status.TRACKED || status == Status.EXTENDED_TRACKED || status == Status.LIMITED;
    }

    void UpdateVisibility()
    {
        // Check if any model target or its guide view is currently being rendered, in that case we hide the instructions
        foreach (var modelTargetBehaviour in mModelTargetBehaviours)
        {
            if (IsGuideViewRendered(modelTargetBehaviour.TargetStatus.StatusInfo))
            {
                Target.SetActive(false);
                mIsRenderingGuideView = true;
                return;
            }

            if (ShouldBeRendered(modelTargetBehaviour.TargetStatus.Status))
            {
                Target.SetActive(false);
                mTargetsAreRendered = true;
                return;
            }
        }

        Target.SetActive(true);
        mIsRenderingGuideView = false;
        mTargetsAreRendered = false;
    }
}
