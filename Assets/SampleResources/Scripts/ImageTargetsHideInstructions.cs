/*===============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using Vuforia;

public class ImageTargetsHideInstructions : MonoBehaviour
{
    public GameObject Target;

    ImageTargetBehaviour[] mImageTargetBehaviours;
    bool mTargetsAreRendered;

    public void Awake()
    {
        mImageTargetBehaviours = FindObjectsOfType<ImageTargetBehaviour>();
    }

    public void Start()
    {
        // Listen for any status changes of the image targets
        foreach (var imageTargetBehaviour in mImageTargetBehaviours)
        {
            imageTargetBehaviour.OnTargetStatusChanged += TargetStatusChanged;
            imageTargetBehaviour.OnBehaviourDestroyed += BehaviourDestroyed;
        }
    }

    void BehaviourDestroyed(ObserverBehaviour behaviour)
    {
        behaviour.OnTargetStatusChanged -= TargetStatusChanged;
        behaviour.OnBehaviourDestroyed -= BehaviourDestroyed;
    }

    public void TargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        // If the rendering state changes we update the visibility of the instructions
        if (ShouldBeRendered(status.Status) != mTargetsAreRendered)
            UpdateVisibility();
    }

    bool ShouldBeRendered(Status status)
    {
        if (status == Status.TRACKED || status == Status.EXTENDED_TRACKED || status == Status.LIMITED)
            return true;

        return false;
    }

    void UpdateVisibility()
    {
        // Check if any image target is currently being rendered, in that case we hide the instructions
        foreach (var imageTargetBehaviour in mImageTargetBehaviours)
        {
            if (ShouldBeRendered(imageTargetBehaviour.TargetStatus.Status))
            {
                Target.SetActive(false);
                mTargetsAreRendered = true;
                return;
            }
        }

        Target.SetActive(true);
        mTargetsAreRendered = false;
    }
}
