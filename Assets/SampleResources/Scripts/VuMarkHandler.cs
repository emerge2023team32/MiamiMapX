/*===============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

/// <summary>
/// A custom handler which uses the VuMarkManager.
/// </summary>
public class VuMarkHandler : MonoBehaviour
{
    [System.Serializable]
    public class AugmentationObject
    {
        public string VuMarkID;
        public GameObject Augmentation;
    }
    
    // Define the number of persistent child objects of the VuMarkBehaviour. When
    // destroying the instance-specific augmentations, it will start after this value.
    // Persistent Children:
    // 1. Canvas -- displays info about the VuMark
    // 2. LineRenderer -- displays border outline around VuMark
    const int PERSISTENT_NUMBER_OF_CHILDREN = 2;
    Dictionary<string, Texture2D> mVuMarkInstanceTextures;
    Dictionary<string, GameObject> mVuMarkAugmentationObjects;
    
    readonly Dictionary<VuMarkBehaviour, bool> mVuMarkBehaviours = new Dictionary<VuMarkBehaviour, bool>();
    
    public AugmentationObject[] AugmentationObjects;

    void Start()
    {
        mVuMarkInstanceTextures = new Dictionary<string, Texture2D>();
        mVuMarkAugmentationObjects = new Dictionary<string, GameObject>();

        foreach (var obj in AugmentationObjects)
            mVuMarkAugmentationObjects.Add(obj.VuMarkID, obj.Augmentation);

        // Hide the initial VuMark Template when the scene starts.
        foreach (var vuMarkBehaviour in FindObjectsOfType<VuMarkBehaviour>())
            ToggleRenderers(vuMarkBehaviour.gameObject, false);
        
        VuforiaApplication.Instance.OnVuforiaStarted += VuforiaStarted;
        VuforiaApplication.Instance.OnVuforiaStopped += VuforiaStopped;
    }

    void VuforiaStarted()
    {
        VuforiaBehaviour.Instance.SetMaximumSimultaneousTrackedImages(10);
        VuforiaBehaviour.Instance.World.OnObserverCreated += OnObserverCreated;
    }

    void VuforiaStopped()
    {
        VuforiaApplication.Instance.OnVuforiaStarted -= VuforiaStarted;
        VuforiaApplication.Instance.OnVuforiaStopped -= VuforiaStopped;

        if (VuforiaBehaviour.Instance != null)
        {
            VuforiaBehaviour.Instance.SetMaximumSimultaneousTrackedImages(4);
            VuforiaBehaviour.Instance.World.OnObserverCreated -= OnObserverCreated;
        }
    }

    void OnObserverCreated(ObserverBehaviour behaviour)
    {
        if (!(behaviour is VuMarkBehaviour vuMarkBehaviour))
            return;

        if (mVuMarkBehaviours.ContainsKey(vuMarkBehaviour))
            return;
        
        mVuMarkBehaviours.Add(vuMarkBehaviour, false);

        vuMarkBehaviour.GetComponent<VuMarkObserverEventHandler>().OnVuMarkFound += OnVuMarkFound;
        vuMarkBehaviour.GetComponent<VuMarkObserverEventHandler>().OnVuMarkLost += OnVuMarkLost;
        vuMarkBehaviour.OnBehaviourDestroyed += OnVuMarkDestroyed;
    }

    void OnVuMarkDestroyed(ObserverBehaviour behaviour)
    {
        behaviour.GetComponent<VuMarkObserverEventHandler>().OnVuMarkFound -= OnVuMarkFound;
        behaviour.GetComponent<VuMarkObserverEventHandler>().OnVuMarkLost -= OnVuMarkLost;
        behaviour.OnBehaviourDestroyed -= OnVuMarkDestroyed;
        mVuMarkBehaviours.Remove((VuMarkBehaviour) behaviour);
    }

    /// <summary>
    ///  Register a callback which is invoked whenever a VuMark-result is newly detected which was not tracked
    ///  in the previous frame
    /// </summary>
    void OnVuMarkFound(VuMarkBehaviour vuMarkBehaviour)
    {
        mVuMarkBehaviours[vuMarkBehaviour] = true;

        if (RetrieveStoredTextureForVuMarkTarget(vuMarkBehaviour) == null)
            mVuMarkInstanceTextures.Add(GetVuMarkId(vuMarkBehaviour), GenerateTextureFromVuMarkInstanceImage(vuMarkBehaviour));
            
        Debug.Log("<color=cyan>VuMarkHandler.OnVuMarkFound(): </color>" + vuMarkBehaviour.TargetName);
        GenerateVuMarkBorderOutline(vuMarkBehaviour);
        ToggleRenderers(vuMarkBehaviour.gameObject, true);
        
        // Check for existence of previous augmentations and delete before instantiating new ones.
        DestroyChildAugmentationsOfTransform(vuMarkBehaviour.transform);
        
        SetVuMarkInfoForCanvas(vuMarkBehaviour);
        SetVuMarkAugmentation(vuMarkBehaviour);
    }

    void OnVuMarkLost(VuMarkBehaviour vuMarkBehaviour)
    {
        if (!mVuMarkBehaviours.TryGetValue(vuMarkBehaviour, out var tracked) || !tracked)
            return;

        mVuMarkBehaviours[vuMarkBehaviour] = false;
        
        Debug.Log("<color=cyan>VuMarkHandler.OnVuMarkLost(): </color>" + GetVuMarkId(vuMarkBehaviour));

        ToggleRenderers(vuMarkBehaviour.gameObject, false);
        DestroyChildAugmentationsOfTransform(vuMarkBehaviour.transform);
    }

    string GetVuMarkDataType(VuMarkBehaviour vuMarkBehaviour)
    {
        switch (vuMarkBehaviour.InstanceId.DataType)
        {
            case InstanceIdType.BYTE:
                return "Bytes";
            case InstanceIdType.STRING:
                return "String";
            case InstanceIdType.NUMERIC:
                return "Numeric";
        }
        return string.Empty;
    }

    string GetVuMarkId(VuMarkBehaviour vuMarkBehaviour)
    {
        switch (vuMarkBehaviour.InstanceId.DataType)
        {
            case InstanceIdType.BYTE:
                return vuMarkBehaviour.InstanceId.HexStringValue;
            case InstanceIdType.STRING:
                return vuMarkBehaviour.InstanceId.StringValue;
            case InstanceIdType.NUMERIC:
                return vuMarkBehaviour.InstanceId.NumericValue.ToString();
        }
        return string.Empty;
    }

    string GetNumericVuMarkDescription(VuMarkBehaviour vuMarkBehaviour)
    {
        if (vuMarkBehaviour.InstanceId.DataType != InstanceIdType.NUMERIC)
            return string.Empty;
        
        // Change the description based on the VuMark ID
        switch (vuMarkBehaviour.InstanceId.NumericValue % 4)
        {
            case 1:
                return "Astronaut";
            case 2:
                return "Drone";
            case 3:
                return "Fissure";
            case 0:
                return "Oxygen Tank";
            default:
                return "Unknown";
        }
    }

    void SetVuMarkInfoForCanvas(VuMarkBehaviour vuMarkBehaviour)
    {
        var canvasText = vuMarkBehaviour.gameObject.GetComponentInChildren<Text>();
        var vuMarkId = GetVuMarkId(vuMarkBehaviour);
        var vuMarkDesc = GetVuMarkDataType(vuMarkBehaviour);
        var vuMarkDataType = GetNumericVuMarkDescription(vuMarkBehaviour);

        canvasText.text =
            "<color=yellow>VuMark Instance Id: </color>" +
            "\n" + vuMarkId + " - " + vuMarkDesc +
            "\n\n<color=yellow>VuMark Type: </color>" +
            "\n" + vuMarkDataType;
    }

    void SetVuMarkAugmentation(VuMarkBehaviour vuMarkBehaviour)
    {
        mVuMarkAugmentationObjects.TryGetValue(GetVuMarkId(vuMarkBehaviour), out var sourceAugmentation);

        if (sourceAugmentation != null)
        {
            var augmentation = Instantiate(sourceAugmentation, vuMarkBehaviour.transform);
            augmentation.transform.localScale = Vector3.one;
        }
    }

    Texture2D RetrieveStoredTextureForVuMarkTarget(VuMarkBehaviour vuMarkBehaviour)
    {
        mVuMarkInstanceTextures.TryGetValue(GetVuMarkId(vuMarkBehaviour), out var texture);
        return texture;
    }

    Texture2D GenerateTextureFromVuMarkInstanceImage(VuMarkBehaviour vuMarkBehaviour)
    {
        if (vuMarkBehaviour.InstanceImage == null)
        {
            Debug.Log("VuMark Instance Image is null.");
            return null;
        }

        var texture = new Texture2D(vuMarkBehaviour.InstanceImage.Width, vuMarkBehaviour.InstanceImage.Height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp
        };

        vuMarkBehaviour.InstanceImage.CopyToTexture(texture);

        return texture;
    }


    void GenerateVuMarkBorderOutline(VuMarkBehaviour vuMarkBehaviour)
    {
        var lineRendererAugmentation = vuMarkBehaviour.GetComponentInChildren<LineRenderer>();
        if (lineRendererAugmentation == null)
        {
            var vuMarkBorder = new GameObject("VuMarkBorder");
            vuMarkBorder.transform.SetParent(vuMarkBehaviour.transform);
            vuMarkBorder.transform.localPosition = Vector3.zero;
            vuMarkBorder.transform.localEulerAngles = Vector3.zero;
            vuMarkBorder.transform.localScale = new Vector3(1 / vuMarkBehaviour.transform.localScale.x,
                                                            1, 1 / vuMarkBehaviour.transform.localScale.z);
            lineRendererAugmentation = vuMarkBorder.AddComponent<LineRenderer>();
            lineRendererAugmentation.enabled = false;
            vuMarkBehaviour.GetComponent<VuMarkObserverEventHandler>()?.AssignLineRenderer(lineRendererAugmentation);
            lineRendererAugmentation.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRendererAugmentation.receiveShadows = false;
            // This shader needs to be added in the Project's Graphics Settings,
            // unless it is already in use by a Material present in the project.
            lineRendererAugmentation.material.shader = Shader.Find("Unlit/Color");
            lineRendererAugmentation.material.color = Color.clear;
            lineRendererAugmentation.positionCount = 4;
            lineRendererAugmentation.loop = true;
            lineRendererAugmentation.useWorldSpace = false;
            var vuMarkSize = vuMarkBehaviour.GetSize();
            var curve = new AnimationCurve();
            curve.AddKey(0.0f, 1.0f);
            curve.AddKey(1.0f, 1.0f);
            lineRendererAugmentation.widthCurve = curve;
            lineRendererAugmentation.widthMultiplier = 0.003f;
            var vuMarkExtentsX = vuMarkSize.x * 0.5f + lineRendererAugmentation.widthMultiplier * 0.5f;
            var vuMarkExtentsZ = vuMarkSize.y * 0.5f + lineRendererAugmentation.widthMultiplier * 0.5f;
            lineRendererAugmentation.SetPositions(new []
                                       {
                                           new Vector3(-vuMarkExtentsX, 0.001f, vuMarkExtentsZ),
                                           new Vector3(vuMarkExtentsX, 0.001f, vuMarkExtentsZ),
                                           new Vector3(vuMarkExtentsX, 0.001f, -vuMarkExtentsZ),
                                           new Vector3(-vuMarkExtentsX, 0.001f, -vuMarkExtentsZ)
                                       });
        }
        
        var lineRendererComponent = vuMarkBehaviour.GetComponent<VuMarkObserverEventHandler>();
        if(lineRendererComponent != null) 
            lineRendererComponent.AssignLineRenderer(lineRendererAugmentation);
    }

    void DestroyChildAugmentationsOfTransform(Transform parent)
    {
        if (parent.childCount > PERSISTENT_NUMBER_OF_CHILDREN)
        {
            for (var x = PERSISTENT_NUMBER_OF_CHILDREN; x < parent.childCount; x++)
                Destroy(parent.GetChild(x).gameObject);
        }
    }

    void ToggleRenderers(GameObject obj, bool enable)
    {
        var rendererComponents = obj.GetComponentsInChildren<Renderer>(true);
        var canvasComponents = obj.GetComponentsInChildren<Canvas>(true);

        foreach (var component in rendererComponents)
        {
            // Skip the LineRenderer
            if (!(component is LineRenderer))
                component.enabled = enable;
        }

        foreach (var component in canvasComponents)
            component.enabled = enable;
    }
}
