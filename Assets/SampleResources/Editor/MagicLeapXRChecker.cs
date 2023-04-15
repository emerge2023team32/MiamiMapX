/*===============================================================================
Copyright (c) 2022 PTC Inc. All Rights Reserved.

Confidential and Proprietary - Protected under copyright and other laws.
Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/


using System;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

/// <summary>
/// Imports the latest version of the MagicLeap XR package
/// This works around an issue where this dependency is sometimes not updated correctly by Unity
/// </summary>
[InitializeOnLoad]
public static class MagicLeapXRChecker
{
    const string ML_XR_PACKAGE_NAME = "com.unity.xr.magicleap";

    static readonly string sTargetVersion = "7.0.0-exp.3";

    static SearchRequest sSearchRequest;

    static MagicLeapXRChecker()
    {
        EditorApplication.update += OnEditorUpdate;
        sSearchRequest = Client.Search(ML_XR_PACKAGE_NAME);
    }

    static void OnEditorUpdate()
    {
        EditorApplication.update -= OnEditorUpdate;
        if (sSearchRequest == null || !sSearchRequest.IsCompleted)
            return;

        AddLatestCompatibleVersion(sSearchRequest.Result.FirstOrDefault());

        sSearchRequest = null;
    }

    static void AddLatestCompatibleVersion(UnityEditor.PackageManager.PackageInfo packageInfo)
    {
        if (packageInfo == null)
        {
            Debug.LogErrorFormat("Unable to set latest package version for package '{0}'.", ML_XR_PACKAGE_NAME);
            return;
        }

        var compatibleVersions = packageInfo.versions.compatible;
        var targetCompatible = compatibleVersions.SingleOrDefault(versionString => versionString.Equals(sTargetVersion));
        if (targetCompatible == null)
            targetCompatible = compatibleVersions.Last();

        var versionedPackageName = string.Format("{0}@{1}", ML_XR_PACKAGE_NAME, targetCompatible);
        
        Client.Add(versionedPackageName);
    }
}