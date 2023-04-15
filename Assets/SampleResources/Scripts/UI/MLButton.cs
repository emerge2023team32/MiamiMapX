/*===============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using static UnityEngine.UI.Button;

public class MLButton : MonoBehaviour
{
    [SerializeField] public GameObject highlight;
    [SerializeField] public ButtonClickedEvent onClick = new ButtonClickedEvent();

    public bool selected { get; private set; } = false;

    AnimatedFloat buttonDepthPlane;

    public void Awake()
    {
        buttonDepthPlane = new AnimatedFloat(0);
    }

    public void SetSelected(bool value)
    {
        selected = value;

        if (selected)
        {
            buttonDepthPlane.AnimateToValue(1, MLConstants.ANIMATION_TIME);
        }
        else
        {
            buttonDepthPlane.AnimateToValue(0, MLConstants.ANIMATION_TIME);
        }

        if (highlight != null)
        {
            highlight.SetActive(selected);
        }
    }

    public void Update()
    {
        if (buttonDepthPlane.IsAnimating())
        {
            var position = transform.localPosition;
            position.z = -1 * buttonDepthPlane.GetValue() * MLConstants.CANVAS_DEPTH_PLANE_STEP;
            transform.localPosition = position;
        }
    }
}
