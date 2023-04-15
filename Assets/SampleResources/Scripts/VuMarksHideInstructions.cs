/*===============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Vuforia;

public class VuMarksHideInstructions : MonoBehaviour
{
    public GameObject Target;

    readonly List<VuMarkBehaviour> mVuMarkBehaviours = new List<VuMarkBehaviour>();
    bool mVuMarksAreRendered;

    public void Start()
    {
        // Listen for any new VuMark being detected
        VuforiaBehaviour.Instance.World.OnObserverCreated += ObserverCreated;
    }

    public void OnDestroy()
    {
        if (VuforiaBehaviour.Instance != null)
            VuforiaBehaviour.Instance.World.OnObserverCreated -= ObserverCreated;

        foreach (var vuMarkBehaviour in mVuMarkBehaviours.ToList())
            BehaviourDestroyed(vuMarkBehaviour);
    }

    public void ObserverCreated(ObserverBehaviour observerBehaviour)
    {
        var vuMarkBehaviour = observerBehaviour as VuMarkBehaviour;
        if (vuMarkBehaviour == null)
            return;
        
        if (mVuMarkBehaviours.Contains(vuMarkBehaviour))
            return;

        // When a new VuMark is detected, we start listening for its status changes
        mVuMarkBehaviours.Add(vuMarkBehaviour);
        vuMarkBehaviour.OnTargetStatusChanged += TargetStatusChanged;
        vuMarkBehaviour.OnBehaviourDestroyed += BehaviourDestroyed;
    }

    void BehaviourDestroyed(ObserverBehaviour behaviour)
    {
        mVuMarkBehaviours.Remove((VuMarkBehaviour) behaviour);
        behaviour.OnTargetStatusChanged -= TargetStatusChanged;
        behaviour.OnBehaviourDestroyed -= BehaviourDestroyed;
    }

    public void TargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        // If the rendering state changes we update the visibility of the instructions
        if (ShouldBeRendered(status.Status) != mVuMarksAreRendered)
            UpdateVisibility();
    }

    bool ShouldBeRendered(Status status)
    {
        return status == Status.TRACKED || status == Status.EXTENDED_TRACKED || status == Status.LIMITED;
    }

    void UpdateVisibility()
    {
        // Check if any VuMark target is currently being rendered, in that case we hide the instructions
        foreach (var vuMarkBehaviour in mVuMarkBehaviours)
        {
            if (ShouldBeRendered(vuMarkBehaviour.TargetStatus.Status))
            {
                Target.SetActive(false);
                mVuMarksAreRendered = true;
                return;
            }
        }

        Target.SetActive(true);
        mVuMarksAreRendered = false;
    }
}
