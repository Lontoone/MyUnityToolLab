using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectPoint : MonoBehaviour
{
    //GetInstanceID用變，用posisiton做辨識    
    public LevelMapSO data;
#if UNITY_EDITOR
    [ReadOnly]
#endif
    [Tooltip("Make sure id is empty or unique in this scene")]
    public string portSetId;

    public void Awake()
    {
        LevelFlowManager.flowData = data;
    }

    [ContextMenu("reset id")]
    private void ResetPortId()
    {
        portSetId = "";
    }


    #region OnValidate Check
#if (UNITY_EDITOR)
    //Works for: AddComponent, Component Context Menu 'Reset', Component Context Menu 'Paste Component As New'
    private void Reset()
    {
        if (Event.current != null)
        {
            Debug.Log("Reset - Type: " + Event.current.type.ToString());
        }
        else
        {
            //Component Context Menu 'Paste Component As New'
            Debug.Log("Reset - EventSystem: null");
        }
    }

    private void OnValidate()
    {
        //ref: https://forum.unity.com/threads/callback-when-object-is-created-in-editor.190223/
        if (Event.current != null)
        {
            if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "Duplicate")
            {
                //Ctrl+D
                //Main Menu 'Edit > Duplicate'
                Debug.Log("ExecuteCommand - Duplicate");
                //重設 id
                portSetId = "";

            }
            else if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "Paste")
            {
                //Ctrl+V
                //Main Menu 'Edit > Paste'
                Debug.Log("ExecuteCommand - Paste");
                portSetId = "";
            }
            else if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "Paste")
            {
                //Never seems to occur...
                Debug.Log("ValidateCommand - Paste");
                portSetId = "";
            }
        }
    }
#endif
    #endregion
}
