using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadFirstSceneControl : MonoBehaviour
{
    public LevelMapSO data;
    private IEnumerator Start()
    {
        //NOTE: You can do a check-point loading to 
        //load to the certain scene.
        yield return new WaitForFixedUpdate();
        LevelFlowManager.flowData = data;
        LevelFlowManager.LoadFromStart("StartPoint");
    }
}
