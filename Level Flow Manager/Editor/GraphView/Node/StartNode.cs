using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class StartNode : BaseNode
{
    public PortSet portSet;
    //Enter name Code
    public string startName;
    TextField startNameTextField;

    //TODO: Enter title style

    public StartNode() { }
   
    public StartNode(Vector2 _position, LevelEditorWindow _editorWindow, LevelFlowGraphView _graphView)
    {
        title = "Enter";
        SetPosition(new Rect(_position, defaultNodeSize));

        UnityEngine.UIElements.Label label_statName = new UnityEngine.UIElements.Label("name");
        mainContainer.Add(label_statName);

        startNameTextField = new TextField(""); //傳入label標籤，會跟對應到的label同行，否則獨立一行
        startNameTextField.RegisterValueChangedCallback(value =>
        {
            startName = value.newValue;
        });

        startNameTextField.SetValueWithoutNotify(startName);
        mainContainer.Add(startNameTextField);

        _graphView.AddToClassList("GraphViewStyleSheet");

        SetUpPort();

        RefreshExpandedState();
        RefreshPorts();
    }

    public override void LoadValueIntoField()
    {
        startNameTextField.SetValueWithoutNotify(startName);
        ClearPortSet();     
        SetUpPort();     
    }
    public override void ClearPortSet()
    {
        for (int i = 0; i < outPorts.Count; i++)
        {
            outPorts[i].RemoveFromHierarchy();
        }
        inPorts.Clear();
        outPorts.Clear();
    }

    private void SetUpPort()
    {
        //clear port
        if (portSet == null)
        {
            portSet = new PortSet(nodeGuid, Vector2.zero, "Start");
        }
        Port outputPort = AddOutputPort("Start", Port.Capacity.Single);
        portSet.nodeGuid = nodeGuid;
        outputPort.name = portSet.localOutGuid;
        outputContainer.Add(outputPort);
    }
}

