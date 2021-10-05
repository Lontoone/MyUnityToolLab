using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Threading.Tasks;
using System.IO;

//TODO: Load all Scene
//TODO: OnConnectPointEntered event not working
//TODO: Unload scenes except current and main
//TODO: [Bug]Edge breaks when sceneUpdated
//TODO: More API?

public class LevelFlowManager
{
    public static LevelMapSO flowData;
    //public static event Action<string> OnLevelEntered;
    private static string loadingScene;
    public static event Action<string> OnConnectPointEntered;
    private static bool load_isDone = false;
    private static bool inited = false;
    public static void Init()
    {
        if (!inited)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            inited = true;
        }
    }
    public static async void LoadFromStart(string _startName)
    {
        Init();
        //Find Stat Node with this name:
        StartNodeData _data = flowData.startNodeDatas.Find(x => x.startName == _startName);
        //StartNodeData _data = _flowData.startNodeDatas.Find(x => x.startName == _startName);
        if (_data != null)
        {
            LoadNextScene(_data.portSet.setGuid);
        }
        else
        {
            //Can't log the map
            Debug.LogError("Start node named " + _startName + " not found.");
        }
    }

    public static async void LoadNextScene(string _setGuid)
    {
        Init();
        if (flowData == null)
        {
            Debug.Log("flow Data unset");
            return;
        }
        //Get this portSet
        PortSet _thisPortSet = flowData.GetPortSet(_setGuid);
        if (_thisPortSet == null)
        {
            //  => Not on the data : return
            Debug.Log("Port " + _setGuid + " is Null");
        }
        else
        {
            //Get this port's next port in link data
            PortSet _nextPortSet = flowData.GetLinkedPort(_thisPortSet.localOutGuid);
            if (_nextPortSet != null)
            {
                //Get Next port's scene data and enterPoint object
                LevelNodeData _node = (LevelNodeData)flowData.GetNode(_nextPortSet.nodeGuid);

                //Apply scene load Mode
                if (_node.asyncType == AsyncLoadType.Async)
                {
                    await LoadSceneAsync(_node.GetScenePath(), _nextPortSet.setGuid, _node.loadType);
                }
                else
                {
                    LoadSceneSync(_node.GetScenePath(), _nextPortSet.setGuid, _node.loadType);
                }

                loadingScene = _node.GetScenePath();
            }
            else
            {
                Debug.Log("Next Port " + " is Null");
            }

        }
    }
    private static async void LoadSceneSync(string _scenePath, string _nextGuid, SceneLoadType _type)
    {
#if UNITY_EDITOR
        //SceneAsset _sceneToLoad = AssetDatabase.LoadAssetAtPath<SceneAsset>(_scenePath);
        string _sceneToLoad = Path.GetFileNameWithoutExtension(_scenePath);
#else
        string _sceneToLoad = Path.GetFileNameWithoutExtension(_scenePath);  
#endif
        if (SceneManager.GetSceneByPath(_scenePath).isLoaded || _scenePath == loadingScene)
        {
            Debug.Log(_sceneToLoad + "  is Loaded");
            return;
        }


        Debug.Log("Sync Load Scene -name: " + _sceneToLoad);
        if (_type == SceneLoadType.Additive)
        {
            SceneManager.LoadScene(_sceneToLoad, LoadSceneMode.Additive);
            InvokeEnterPoint(_nextGuid);
        }
        else if (_type == SceneLoadType.Single)
        {
            //有固定場景時

            if (flowData.mainSceneName != "")
            {
                AsyncUnloadAllScenesExcept(flowData.mainSceneName, _sceneToLoad);
                SceneManager.LoadScene(_sceneToLoad, LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.LoadScene(_sceneToLoad, LoadSceneMode.Single);
            }

            InvokeEnterPoint(_nextGuid);
        }
    }

    private static async Task LoadSceneAsync(string _scenePath, string _nextGuid, SceneLoadType _type)
    {
#if UNITY_EDITOR
        //SceneAsset _sceneToLoad = AssetDatabase.LoadAssetAtPath<SceneAsset>(_scenePath);
        string _sceneToLoad = Path.GetFileNameWithoutExtension(_scenePath);
#else
        string _sceneToLoad = Path.GetFileNameWithoutExtension(_scenePath);  
#endif

        if (SceneManager.GetSceneByPath(_scenePath).isLoaded || loadingScene == _scenePath)
        {
            Debug.Log(_sceneToLoad + "  is Loaded");
            return;
        }
        loadingScene = _scenePath;

        Debug.Log("Load Scene -name: " + _sceneToLoad);
        if (_type == SceneLoadType.Additive)
        {
            SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Additive);
            /*
            await WaitForAsyncScene(
                    _sceneToLoad.name,
                    LoadSceneMode.Additive,
                    async delegate { InvokeEnterPoint(_nextGuid); }
                    );*/
        }
        else if (_type == SceneLoadType.Single)
        {
            //[TODO]:有基本場景
            //To keep the main scene, use fake single mode

            if (flowData.mainSceneName != "")
            {
                await AsyncUnloadAllScenesExcept(flowData.mainSceneName, _sceneToLoad);
                SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Single);
            }

        }
        InvokeEnterPoint(_nextGuid);
    }

    private static async Task AsyncUnloadAllScenesExcept(params string[] sceneName)
    {
        int c = SceneManager.sceneCount;
        for (int i = 0; i < c; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            //if (scene.name != sceneName)
            //Debug.Log("Unload " + scene.name + " vs " + sceneName[i] + (!sceneName.Contains(scene.name)));
            if (!sceneName.Contains(scene.name))
            {
                SceneManager.UnloadSceneAsync(scene);
            }
        }
    }
    private static async void InvokeEnterPoint(string _guid)
    {
        string _targetScenePath = ((LevelNodeData)flowData.GetNode(flowData.GetPortSet(_guid).nodeGuid)).GetScenePath();
        Scene _targetScene = SceneManager.GetSceneByPath(_targetScenePath);

        while (!_targetScene.isLoaded)
        {
            await Task.Yield();
        }

        loadingScene = "";
        OnConnectPointEntered?.Invoke(_guid);

    }
    /*
    private static async Task WaitForAsyncScene(string sceneName, LoadSceneMode _loadMode, Action _callback)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, _loadMode);

        //while (!async.isDone)
        while (!load_isDone)
        {
            //Debug.Log("scene loading ");
           await Task.Yield();
        }
        _callback?.Invoke();
        load_isDone = false;

    }*/

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        load_isDone = true;
    }



}

