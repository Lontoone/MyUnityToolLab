using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LevelEditorWindow : EditorWindow
{
    public LevelMapSO flowData;
    private LevelFlowGraphView graphView;
    private ToolbarMenu toolbarMenu;
    private MapSaveLoad mapSaveLoad;

    [OnOpenAsset(1)] //當資料夾裡的檔案被點擊時的callback
    public static bool ShowWindowInfo(int _instanceID, int line) //每個project裡面的檔案都有自己的id
    {
        UnityEngine.Object item = EditorUtility.InstanceIDToObject(_instanceID);

        if (item is LevelMapSO) //點擊這類檔案的資料，開啟對應的編輯視窗
        {
            LevelEditorWindow window = (LevelEditorWindow)GetWindow(typeof(LevelEditorWindow));
            window.titleContent = new GUIContent("Level Flow Editor");
            window.flowData = item as LevelMapSO;
            window.minSize = new Vector2(500, 250);
            window.Load();
        }
        return false;
    }


    public void OnEnable()
    {
        ConstructGraphView(); //產生注意順序
        GenerateToolBar();
        Load();
    }

    //建立網格背景
    private void ConstructGraphView()
    {
        graphView = new LevelFlowGraphView(this);
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);

        mapSaveLoad = new MapSaveLoad(graphView);
    }

    private void GenerateToolBar()
    {
        StyleSheet styleSheet = Resources.Load<StyleSheet>("GraphViewStyleSheet");
        rootVisualElement.styleSheets.Add(styleSheet);

        Toolbar toolbar = new Toolbar();

        //存檔btn
        Button save_btn = new Button()
        {
            text = "Save"
        };
        save_btn.clicked += () =>
        {
            Save();
        };
        toolbar.Add(save_btn);

        //讀檔btn
        Button load_btn = new Button()
        {
            text = "Load"
        };
        load_btn.clicked += () =>
        {
            Load();
        };
        toolbar.Add(load_btn);

        //Upadte current Scene
        Button updateCurrentScene_btn = new Button()
        {
            text = "Update current scene"
        };
        updateCurrentScene_btn.clicked += () =>
        {
            SampleCurrentScene();
        };
        toolbar.Add(updateCurrentScene_btn);

        //Load All Scenes
        Button loadAllScene_btn = new Button()
        {
            text = "Load all scene"
        };
        loadAllScene_btn.clicked += () =>
        {
            LoadAllScene();
        };
        toolbar.Add(loadAllScene_btn);

        //Close other scenes
        Button closeOtherScenes_btn = new Button()
        {
            text = "Close scenes"
        };
        closeOtherScenes_btn.clicked += () =>
        {
            CloseOtherScene();
        };
        toolbar.Add(closeOtherScenes_btn);

        rootVisualElement.Add(toolbar);
    }

    private void Save()
    {
        if (flowData != null)
        {
            mapSaveLoad.Save(flowData);
        }
    }
    private void Load()
    {
        if (flowData != null)
        {
            Debug.Log("load");
            mapSaveLoad.Load(flowData);
        }
    }

    //***SAMPLE SCENE***
    private void SampleCurrentScene()
    {
        //get current Scene 
        Scene _currentScene = SceneManager.GetActiveScene();

        //get current Scene's node
        LevelNode _currentNode = LevelNode.FindNodeByScene(_currentScene);

        //create one if not exist
        if (_currentNode == null)
        {
            return;
        }
        //_currentNode.ClearLevelConnectPort();
        _currentNode.ClearPortSet();

        //get all the enter point
        ConnectPoint[] points = FindObjectsOfType<ConnectPoint>();
        Debug.Log(points.Length);
        _currentNode.RemoveNoExistingPortSet(points);

        Vector3[] _projectedPoints = ProjectPointsToPlane(new Vector3(0, 1, 0), points);

        //This Scene contains no connect point
        if (_projectedPoints.Length < 1) { return; }

        //sample points' distance to sub view
        Vector3[] _subViewPoints = ProjectToSubView(_projectedPoints);

        for (int i = 0; i < _subViewPoints.Length; i++)
        {
            //create port
            //PortSet _set = _currentNode.CreateLevelPort(_subViewPoints[i], points[i].id, points[i].name);
            PortSet _set = _currentNode.CreateLevelPort(_subViewPoints[i], points[i].portSetId, points[i].name);
            points[i].portSetId = _set.setGuid;
        }

        //不mark dirty不能存檔
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

    }
    private Vector3[] ProjectPointsToPlane(Vector3 _heightAxist, ConnectPoint[] _points)
    {
        Vector3[] _projectedPoints = new Vector3[_points.Length];

        Vector3 _widthProjectAxis = new Vector3(1, 0, 0);
        Vector3 _heightProjectAxis = _heightAxist;
        for (int i = 0; i < _points.Length; i++)
        {
            Vector3 _projectPoint = _points[i].transform.position;
            _projectPoint.x = Vector3.Dot(_projectPoint, _widthProjectAxis);
            _projectPoint.y = Vector3.Dot(_projectPoint, _heightProjectAxis);
            _projectedPoints[i] = _projectPoint;
        }

        return _projectedPoints;

    }
    private Vector3[] ProjectToSubView(Vector3[] _projectedPoints)
    {
        if (_projectedPoints.Length < 2)
        {
            return new Vector3[] { Vector3.zero };
        }
        float subMapViewSize = LevelNode.SUB_MAP_VIEW_SIZE - 50;
        float _maxHeight = _projectedPoints.ToList().Max(i => i.y);
        float _minHeight = _projectedPoints.ToList().Min(i => i.y);

        float _maxWidth = _projectedPoints.ToList().Max(i => i.x);
        float _minWidth = _projectedPoints.ToList().Min(i => i.x);

        float _widthGap = _maxWidth - _minWidth;
        float _heightGap = _maxHeight - _minHeight;

        //sample points' distance to sub view
        Vector3[] _subViewPoints = new Vector3[_projectedPoints.Length];
        for (int i = 0; i < _subViewPoints.Length; i++)
        {
            _subViewPoints[i] = new Vector2(
                    (_projectedPoints[i].x - _minWidth) / _widthGap * subMapViewSize,
                    (_projectedPoints[i].y - _minHeight) / _heightGap * subMapViewSize
                );

            //UI 需要翻轉y軸
            _subViewPoints[i].y = subMapViewSize - _subViewPoints[i].y;
        }
        return _subViewPoints;
    }

    //***LOAD ALL SCENE***
    private void LoadAllScene()
    {
        //for each level node's scene data
        for (int i = 0; i < flowData.levelNodeDatas.Count; i++)
        {
            //EditorSceneManager.OpenScene(flowData.levelNodeDatas[i].sceneAssestGuid, OpenSceneMode.Additive);
            EditorSceneManager.OpenScene(flowData.levelNodeDatas[i].GetScenePath(), OpenSceneMode.Additive);
        }
    }

    //***Close Otehr Scene***
    private void CloseOtherScene()
    {
        Scene _currentScene = SceneManager.GetActiveScene();
        for (int i = 0; i < flowData.levelNodeDatas.Count; i++)
        {
            if (flowData.levelNodeDatas[i].GetScenePath() != _currentScene.path)
            {
                //SceneAsset _scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(flowData.levelNodeDatas[i].scenePath);
                Scene _scene = EditorSceneManager.GetSceneByPath(flowData.levelNodeDatas[i].GetScenePath());
                EditorSceneManager.CloseScene(_scene, false);
            }
        }
    }
}
