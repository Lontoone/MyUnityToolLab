using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelFlowGraphView : GraphView
{
    private string styleSheetName = "GraphViewStyleSheet";
    private LevelEditorWindow editorWindow;
    private NodeSearchWindow searchWindow;

    public LevelFlowGraphView(LevelEditorWindow _editorWindow)
    {
        editorWindow = _editorWindow;

        StyleSheet tmpStyleSheet = Resources.Load<StyleSheet>(styleSheetName);
        styleSheets.Add(tmpStyleSheet);

        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new FreehandSelector());

        GridBackground grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
        AddSearchWindow();
    }
    private void AddSearchWindow()
    {
        searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        searchWindow.Configure(editorWindow, this);
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
    }

    //Add node:
    public LevelNode CreateLevelNode(Vector2 _pos)
    {
        LevelNode tmp = new LevelNode(_pos, editorWindow, this);
        return tmp;
    }
    public StartNode CreateStartNode(Vector2 _pos)
    {
        StartNode tmp = new StartNode(_pos, editorWindow, this);
        return tmp;
    }   

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();
        Port startPortView = startPort;
        //ports 會取得全部的port
        ports.ForEach((port) =>
        {
            Port _portView = port;
            if (startPortView != _portView &&  //不能自己連自己
                startPortView.node != _portView.node &&  //不能自己連自己的sub node
                startPortView.direction != port.direction  //out不能連out in不能連in
                )
            {
                //連接
                compatiblePorts.Add(port);
            }
        });
        return compatiblePorts;
    }
}
