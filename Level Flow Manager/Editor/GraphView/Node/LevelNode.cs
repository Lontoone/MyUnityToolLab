using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LevelNode : BaseNode
{
    public const float SUB_MAP_VIEW_SIZE = 250;
    public List<PortSet> portSets = new List<PortSet>();

    public AsyncLoadType asyncType = AsyncLoadType.Async;
    public SceneLoadType loadType = SceneLoadType.Single;
    public object scene;
    public string scenAssetGuid;
    public string scenePath;

    public Label subGraphView;
    private ObjectField sceneField;
    private EnumField loadTypeField;
    private EnumField asyncTypeField;


    public LevelNode() { }
    public LevelNode(Vector2 _position, LevelEditorWindow _editorWindow, LevelFlowGraphView _graphView)
    {
        SetPosition(new Rect(_position, defaultNodeSize));

        CreateSubGraphView();

        //Scene Object field
        sceneField = new ObjectField
        {
            objectType = typeof(Scene),
            allowSceneObjects = false
        };

        sceneField.RegisterValueChangedCallback(ValueTuple =>
        {
            scene = ValueTuple.newValue;
            title = ValueTuple.newValue.name;
            scenAssetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(ValueTuple.newValue as SceneAsset));
            scenePath = AssetDatabase.GetAssetPath(ValueTuple.newValue as SceneAsset);
            //SetScenePath();
        });
        mainContainer.Add(sceneField);


        //Async? Enum field
        asyncTypeField = new EnumField()
        {
            value = asyncType
        };
        asyncTypeField.Init(asyncType);

        asyncTypeField.RegisterValueChangedCallback((value) =>
        {
            //賦予新value
            asyncType = (AsyncLoadType)value.newValue;
        });

        asyncTypeField.SetValueWithoutNotify(asyncType);

        mainContainer.Add(asyncTypeField);

        //Load Type Enum field
        loadTypeField = new EnumField()
        {
            value = loadType
        };
        loadTypeField.Init(loadType);

        loadTypeField.RegisterValueChangedCallback((value) =>
        {
            //賦予新value
            loadType = (SceneLoadType)value.newValue;
        });

        loadTypeField.SetValueWithoutNotify(loadType);

        mainContainer.Add(loadTypeField);



        //新增節點須refresh
        RefreshExpandedState();
        RefreshPorts();
    }

    private void CreateSubGraphView()
    {
        subGraphView = new Label("");
        subGraphView.AddToClassList("subViewBox");
        mainContainer.Add(subGraphView);
    }
    public void SetupSubPortSets()
    {
        //TODO:整理API
        if (portSets != null)
        {
            for (int i = 0; i < portSets.Count; i++)
            {
                Port[] _ports = CreatePortSet(portSets[i].position, portSets[i].pointObjectName);
                _ports[0].name = portSets[i].localInGuid;
                _ports[1].name = portSets[i].localOutGuid;
            }
        }

        //Debug.Log("2. SetupSubPortSets  port count" + portSets.Count);
    }
    private Port[] CreatePortSet(Vector2 _pos, string _title = "port")
    {
        Port _outPort = AddOutputPort("out", Port.Capacity.Single);
        Port _inPort = AddInputPort("in", Port.Capacity.Multi);
        Label textLabel = new Label(_title);

        _inPort.Add(_outPort);
        _inPort.Add(textLabel);
        subGraphView.contentContainer.Add(_inPort);

        _inPort.SetPosition(new Rect(_pos.x, _pos.y, 85, 50));

        inPorts.Add(_inPort);
        outPorts.Add(_outPort);

        return new Port[] { _inPort, _outPort };
    }
    public PortSet CreateLevelPort(Vector2 _pos, string _objSetId, string _title = "port")
    {
        //Check if _objId has port 檢查該對應物件是否已紀錄
        PortSet _portSet = portSets.Find(x => x.setGuid == _objSetId);
        Debug.Log("3. CreateLevelPort  port count" + portSets.Count);
        if (_portSet != null)
        {
            Debug.Log("port esisted " + _portSet.setGuid + " tilie " + _title + " pos " + _pos);
            Port[] ports = CreatePortSet(_pos, _title);
            ports[0].name = _portSet.localInGuid;
            ports[1].name = _portSet.localOutGuid;
            _portSet.pointObjectName = _title;
            //_portSet.position = _pos;
            _portSet.position = new Vector2(Math.Abs(_pos.x), Math.Abs(_pos.y));

            Debug.Log("port count " + outPorts.Count);
        }
        else
        {
            //else =>create new 否則創造一組
            Port[] ports = CreatePortSet(_pos, _title);

            //Save guid to name
            _portSet = new PortSet(nodeGuid, _pos, _title);
            ports[0].name = _portSet.localInGuid;
            ports[1].name = _portSet.localOutGuid;
            portSets.Add(_portSet);

            Debug.Log("port created " + _portSet.setGuid);
        }
        return _portSet;
    }

    public void ClearLevelConnectPort()
    {
        //TODO: 整理API
        for (int i = 0; i < inPorts.Count; i++)
        {
            inPorts[i].RemoveFromHierarchy();
        }
        inPorts.Clear();
        outPorts.Clear();
        Debug.Log("ClearLevelConnectPort port count " + portSets.Count);
    }

    //讀取時候放入資料
    public override void LoadValueIntoField()
    {
        //sceneField.SetValueWithoutNotify((UnityEngine.Object)(object)AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath));
        sceneField.SetValueWithoutNotify((UnityEngine.Object)(object)AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(scenAssetGuid)));
        asyncTypeField.SetValueWithoutNotify(asyncType);
        loadTypeField.SetValueWithoutNotify(loadType);
        //scene = SceneManager.GetSceneByPath(scenAssetGuid);
        scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(scenAssetGuid));
        //title = Path.GetFileNameWithoutExtension(scenAssetGuid);
        title = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(scenAssetGuid));
        SetupSubPortSets();

        //RefreshExpandedState();
        //RefreshPorts();
    }
    public void RemoveNoExistingPortSet(ConnectPoint[] _points)
    {
        if (portSets.Count < 1)
        {
            return;
        }
        for (int i = 0; i < portSets.Count; i++)
        {
            bool _isDiff = true;
            for (int j = 0; j < _points.Length; j++)
            {

                if (portSets[i].setGuid == _points[j].portSetId)
                {
                    _isDiff = false;
                    break;
                }
            }
            //刪除不存在的
            if (_isDiff)
            {
                Debug.Log("移除 " + portSets[i].pointObjectName);
                portSets.Remove(portSets[i]);
                //TODO: port刪除也要刪除連結線
            }
        }
    }


    public static LevelNode FindNodeByScene(Scene _targetScene)
    {
        foreach (KeyValuePair<string, BaseNode> _node in BaseNode.nodeMap)
        {
            if (_node.Value is LevelNode &&
               (_node.Value as LevelNode).scene != null)
            {
                if ((_node.Value as LevelNode).sceneField.value.name == _targetScene.name)
                {
                    return (LevelNode)_node.Value;
                }
            }
        }

        return null;
    }
}
/*
[System.Serializable]
public enum SceneLoadType
{
    APPEDN, ONLY
}
*/
