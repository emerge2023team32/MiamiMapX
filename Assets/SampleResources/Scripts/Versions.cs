/*===============================================================================
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other
countries.
===============================================================================*/

using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class Versions : MonoBehaviour
{
    public Text VersionText;

    void Start()
    {
        var vuforiaVersion = VuforiaApplication.GetVuforiaLibraryVersion();
        var unityVersion = Application.unityVersion;
        VersionText.text = $"Vuforia Version: {vuforiaVersion} | Unity Version: {unityVersion}";
    }
}
