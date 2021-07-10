using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadNextSceneButton : ConnectPoint
{
    public void LoadNext()
    {
        LevelFlowManager.LoadNextScene(portSetId);
    }
}
