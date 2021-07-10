using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private LevelEditorWindow editorWindow;
    private LevelFlowGraphView graphView;

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> tree = new List<SearchTreeEntry>() {
            new SearchTreeGroupEntry(new GUIContent("Level Flow"),0),
            new SearchTreeGroupEntry(new GUIContent("Level Node"),1),

            AddNodeSearch("Start Node",new StartNode()),
            AddNodeSearch("Level Node",new LevelNode())
        };

        return tree;
    }
    private SearchTreeEntry AddNodeSearch(string _name, BaseNode _baseNode)
    {
        SearchTreeEntry tmp = new SearchTreeEntry(new GUIContent(_name))
        {
            level = 2,
            userData = _baseNode
        };
        return tmp;
    }

    public void Configure(LevelEditorWindow _editorWindow, LevelFlowGraphView _graphView)
    {
        editorWindow = _editorWindow;
        graphView = _graphView;
    }


    public bool OnSelectEntry(SearchTreeEntry _SearchTreeEntry, SearchWindowContext _context)
    {
        Vector2 mousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(
                 editorWindow.rootVisualElement.parent, _context.screenMousePosition - editorWindow.position.position
             );

        Vector2 grphviewMousePosition = graphView.contentViewContainer.WorldToLocal(mousePosition);
        return CheckForNodeType(_SearchTreeEntry, grphviewMousePosition);
    }

    private bool CheckForNodeType(SearchTreeEntry _searchTreeEntry, Vector2 _pos)
    {
        switch (_searchTreeEntry.userData)
        {
            case LevelNode node:
                graphView.AddElement(graphView.CreateLevelNode(_pos));
                return true;

            case StartNode node:
                graphView.AddElement(graphView.CreateStartNode(_pos));
                return true;

            default:
                break;
        }

        return false;
    }
}
