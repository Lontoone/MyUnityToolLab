using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Level/LevelFlow")]
public class LevelMapSO : ScriptableObject
{
    //public SceneAsset mainScene;
    //public Scene mainScene;
    public string mainSceneName;

    //[HideInInspector]
    public List<StartNodeData> startNodeDatas = new List<StartNodeData>();
    //[HideInInspector]
    public List<LevelNodeData> levelNodeDatas = new List<LevelNodeData>();
    //[HideInInspector]
    public List<PortSet> allPortSets
    {
        get
        {
            List<PortSet> _temp = new List<PortSet>();
            for (int i = 0; i < levelNodeDatas.Count; i++)
            {
                _temp.AddRange(levelNodeDatas[i].portSets);
            }
            for (int i = 0; i < startNodeDatas.Count; i++)
            {
                _temp.Add(startNodeDatas[i].portSet);
            }
            return _temp;
        }
    }
    [HideInInspector]
    public List<LinkData> linkDatas = new List<LinkData>();

    public BaseNodeData GetNode(string _nodeId)
    {
        return levelNodeDatas.Find(x => x.guid == _nodeId);
    }
    public PortSet GetPortSet(string _setGuid)
    {
        return allPortSets.Find(x => x.setGuid == _setGuid);
    }
    public PortSet GetLinkedPort(string _outPortGuid)
    {
        LinkData _link = linkDatas.Find(x => x.outPortGuid == _outPortGuid);
        if (_link == null)
        {
            return null;
        }
        PortSet _inPortSet = allPortSets.Find(x => x.localInGuid == _link.inPortGuid);
        return _inPortSet;
    }
}

//節點node資料
[System.Serializable]
public class BaseNodeData
{
    //public string inNodeGuid;
    //public string outNodeGuid;
    public string guid;
    public Vector2 position;
}
[System.Serializable]
public class StartNodeData : BaseNodeData
{
    public PortSet portSet = new PortSet();
    public string startName;
}
[System.Serializable]
public class LevelNodeData : BaseNodeData
{
    public List<PortSet> portSets = new List<PortSet>();
    public SceneLoadType loadType = new SceneLoadType();
    public AsyncLoadType asyncType = new AsyncLoadType();
    public string scenePath;
    public string sceneAssestGuid;
    public string GetScenePath()
    {
#if UNITY_EDITOR
        //return SceneManager.GetSceneByPath
        //return AssetDatabase.GUIDToAssetPath(sceneAssestGuid);
        return scenePath;
#else
        return scenePath;
#endif
    }
    /*
    public SceneAsset GetSceneAsset() {
        return AssetDatabase.LoadAssetAtPath<SceneAsset>(GetScenePath());
    }*/

}
[System.Serializable]
public class PortSet
{
    public string setGuid;
    public string localInGuid;
    public string localOutGuid;
    //public string pointObjectInstanceId;
    public string pointObjectName;
    public string nodeGuid;
    public Vector2 position;

    public PortSet() { }
    public PortSet(string _nodeId, Vector2 _pos, string _pointObjName)
    {
        setGuid = Guid.NewGuid().ToString();
        localInGuid = Guid.NewGuid().ToString();
        localOutGuid = Guid.NewGuid().ToString();
        //pointObjectInstanceId = _pointObjId;
        nodeGuid = _nodeId;
        position = _pos;
        pointObjectName = _pointObjName;
    }
}

[System.Serializable]
public class LinkData
{
    public string inPortGuid;
    public string outPortGuid;
}