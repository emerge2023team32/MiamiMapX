using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class MiamiMapController : MonoBehaviour
{
    private GameObject miamiMap;

    void Start()
    {
        miamiMap = GameObject.FindWithTag("MiamiMap");
        miamiMap.SetActive(false);
        Application.logMessageReceived += HandleLog;

    }
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        Debug.LogFormat("[MapX] {0}", logString);
    }
    void Update()
    {
        var targetStatus = GetComponent<ImageTargetBehaviour>().TargetStatus;
        if (miamiMap != null && miamiMap.activeSelf == false && targetStatus.Status == Status.TRACKED)
        {
            miamiMap.SetActive(true);
            miamiMap.GetComponent<CanvasGroup>().interactable = true;
            Debug.Log("Image target is being tracked.");
        }
        else if (miamiMap != null && miamiMap.activeSelf == true && targetStatus.Status != Status.TRACKED)
        {
            miamiMap.SetActive(false);
            miamiMap.GetComponent<CanvasGroup>().interactable = false;
            Debug.Log("Image target is lost.");
        }
    }
}
