using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSaveLoad
{
    private LevelFlowGraphView graphView;
    private List<Edge> edges => graphView.edges.ToList();
    private List<BaseNode> nodes => graphView.nodes.ToList().Where(node => node is BaseNode).Cast<BaseNode>().ToList();

    public MapSaveLoad(LevelFlowGraphView _graphView)
    {
        graphView = _graphView;
    }

    public void Save(LevelMapSO _levelFlowSO)
    {
        SaveEdges(_levelFlowSO);
        SaveNodes(_levelFlowSO);

        //要存檔code修改過的scriptable Object，需要先setDirty告知此檔案被修改了，
        //再SaveAssets
        EditorUtility.SetDirty(_levelFlowSO);
        AssetDatabase.SaveAssets();
    }
    #region Save
    private void SaveEdges(LevelMapSO _levelFlowSO)
    {
        _levelFlowSO.linkDatas.Clear();
        Edge[] connectedEdges = edges.Where(edge => edge.input.node != null).ToArray();
        foreach (Edge _edge in connectedEdges)
        {
            LinkData _data = new LinkData();
            _data.inPortGuid = _edge.input.name;
            _data.outPortGuid = _edge.output.name;
            _levelFlowSO.linkDatas.Add(_data);
        }
    }
    private void SaveNodes(LevelMapSO _levelFlowSO)
    {
        _levelFlowSO.levelNodeDatas.Clear();
        _levelFlowSO.startNodeDatas.Clear();
        Debug.Log("Save Nodes ");
        foreach (BaseNode _node in nodes)
        {
            switch (_node)
            {
                case LevelNode _lvNode:
                    _levelFlowSO.levelNodeDatas.Add(SaveNodeData(_lvNode));
                    break;
                case StartNode _startNode:
                    _levelFlowSO.startNodeDatas.Add(SaveNodeData(_startNode, _levelFlowSO));
                    break;
            }
        }
    }

    private LevelNodeData SaveNodeData(LevelNode _lvnode)
    {
        LevelNode _node = (LevelNode)_lvnode;
        Debug.Log("SAVE portset count " + _node.portSets.Count);
        LevelNodeData _data = new LevelNodeData
        {
            guid = _node.NodeGuid,
            position = _node.GetPosition().position,
            portSets = _node.portSets,
            loadType = _node.loadType,
            asyncType = _lvnode.asyncType,
            scenePath = _lvnode.scenePath,
            //scene = _node.scene,
            sceneAssestGuid = _node.scenAssetGuid,

        };
        return _data;

    }

    private StartNodeData SaveNodeData(StartNode _lvnode, LevelMapSO _levelFlowSO)
    {
        StartNode _node = (StartNode)_lvnode;

        StartNodeData _data = new StartNodeData
        {
            guid = _node.NodeGuid,
            position = _node.GetPosition().position,
            portSet = _node.portSet,
            startName = _node.startName
            //nextInPortGuid = _levelFlowSO.linkDatas.Find(x => x.outPortGuid == _node.).inPortGuid
        };
        return _data;

    }
    #endregion

    public void Load(LevelMapSO _levelFlowSO)
    {
        ClearGraph();
        GenerateNodes(_levelFlowSO);
        ConnectNodes(_levelFlowSO);
    }
    #region Load

    #endregion

    private void ClearGraph()
    {
        edges.ForEach(edge => graphView.RemoveElement(edge));
        nodes.ForEach(node => graphView.RemoveElement(node));
    }
    private void GenerateNodes(LevelMapSO _levelFlowSO)
    {
        //Start Data 
        foreach (StartNodeData _data in _levelFlowSO.startNodeDatas)
        {
            StartNode _startNode = graphView.CreateStartNode(_data.position);
            _startNode.NodeGuid = _data.guid;
            _startNode.startName = _data.startName;
            _startNode.portSet = _data.portSet;
            _startNode.LoadValueIntoField();

            graphView.AddElement(_startNode);
        }
        //Level Node
        foreach (LevelNodeData _data in _levelFlowSO.levelNodeDatas)
        {
            LevelNode _lvNode = graphView.CreateLevelNode(_data.position);
            _lvNode.NodeGuid = _data.guid;
            //_lvNode.scene = _data.scene;
            //_lvNode.scenePath = _data.scenePath;
            _lvNode.scenAssetGuid = _data.sceneAssestGuid;
            _lvNode.asyncType = _data.asyncType;
            _lvNode.loadType = _data.loadType;
            _lvNode.portSets = _data.portSets;
            _lvNode.scenePath = _data.scenePath;
            Debug.Log("1. GenerateNodes  port count" + _data.portSets.Count);
            _lvNode.LoadValueIntoField();

            graphView.AddElement(_lvNode);
        }
    }
    private string CheckScenePath(string _current)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(_current) == null)
        {
            Scene _scene = SceneManager.GetSceneByName(Path.GetFileNameWithoutExtension(_current));
            if (_scene != null)
                return _scene.path;
            else
                return "";
        }
        else
        {
            return _current;
        }
    }

    private void ConnectNodes(LevelMapSO _levelFlowSO)
    {
        List<PortSet> _allPortSets = _levelFlowSO.allPortSets;
        for (int i = 0; i < _allPortSets.Count; i++)
        {
            List<LinkData> connections = _levelFlowSO.linkDatas.Where(edge => edge.outPortGuid == _allPortSets[i].localOutGuid).ToList();
            //List<LinkData> connections = _levelFlowSO.linkDatas.Where(edge => edge.inPortGuid == _allPortSets[i].localInGuid).ToList();
            for (int j = 0; j < connections.Count; j++)
            {
                PortSet _outPortSet = _allPortSets.Find(x => x.localOutGuid == connections[j].outPortGuid);
                PortSet _inPortSet = _allPortSets.Find(x => x.localInGuid == connections[j].inPortGuid);

                BaseNode _outNode = (BaseNode)nodes.Find(x => x.NodeGuid == _outPortSet.nodeGuid);
                BaseNode _inNode = (BaseNode)nodes.Find(x => x.NodeGuid == _inPortSet.nodeGuid);

                //Port _outPort = _outNode.outPorts.Find(x => x.name == connections[j].outPortGuid);
                //Port _inPort = _inNode.inPorts.Find(x => x.name == connections[j].inPortGuid);
                Port _outPort = _outNode.outPorts.Find(x => x.name == connections[j].outPortGuid);
                Port _inPort = _inNode.inPorts.Find(x => x.name == connections[j].inPortGuid);
                LinkNodesTogether(_outPort, _inPort);
            }
        }
    }

    private void LinkNodesTogether(Port _outputPort, Port _inputPort)
    {
        Edge tempEdge = new Edge()
        {
            output = _outputPort,
            input = _inputPort
        };
        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);
        graphView.Add(tempEdge);
    }
}
