/*

Author: Lontoone
Welcome to my blog: https://lontoone.github.io/me
And github: https://github.com/Lontoone
I love you all, please keep creating. <3

*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActionActor;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using System;
using System.Linq;
using System.Xml;

public class Actor : MonoBehaviour
{
    public TextAsset xml;
    public IActor actor;
    private Dictionary<string, dynamic> typeInsctancesMap = new Dictionary<string, dynamic>();

    private Dictionary<string, List<IAction>> triggerActionsMap = new Dictionary<string, List<IAction>>();
    private Dictionary<string, List<IAction>> eventActionsMap = new Dictionary<string, List<IAction>>();
    private static Dictionary<string, List<IAction>> globalEventActionsMap = new Dictionary<string, List<IAction>>();

    private List<IAction> onStartFuncs = new List<IAction>();
    private List<IAction> updateFuncs = new List<IAction>();

    //func , 冷卻時間
    private Dictionary<IAction, float> actCdMaps = new Dictionary<IAction, float>();

    private IAction currentAction;
    private float currentActionDueTime = 0;

    private void Awake()
    {
        //讀取資料
        LoadXml();
    }
    private IEnumerator Start()
    {
        
        WaitForSeconds _wait = new WaitForSeconds(actor.updateTime);

        //On Start 事件
        for (int i = 0; i < onStartFuncs.Count; i++)
        {
            ExeActionWithObjectsData(onStartFuncs[i], new object[] { gameObject });
            //InvokeFunc(onStartFuncs[i], new object[] { gameObject });
        }

        //掛載local事件
        actor.onReceiveEvent += EventTrigger;
        IActor.onGlobalEvent += GlobalEventTrigger;

        //Loop 邏輯
        while (true)
        {
            // Trigger Update event
            for (int i = 0; i < updateFuncs.Count; i++)
            {
                ExeAction(updateFuncs[i]);
                //InvokeFunc(updateFuncs[i]);
            }
            yield return _wait;
        }
    }
    private void EventTrigger(string msg,object[] datas)
    {
        List<IAction> _acts;
        if (eventActionsMap.TryGetValue(msg, out _acts))
        {
            foreach (var act in _acts)
            {
                ExeActionWithObjectsData(act,datas);
            }
        }
    }
    private void GlobalEventTrigger(string msg, object[] datas)
    {
        List<IAction> _acts;
        if (globalEventActionsMap.TryGetValue(msg, out _acts))
        {
            foreach (var act in _acts)
            {
                ExeActionWithObjectsData(act, datas);
            }
        }
    }

    private void Update()
    {
        //========= Trigger 事件 ============
        foreach (KeyValuePair<string, List<IAction>> kp in triggerActionsMap)
        {
            float v = Input.GetAxis(kp.Key);
            if (v == 0)
                continue;

            for (int i = 0; i < kp.Value.Count; i++)
            {
                /*
                kp.Value[i].paras[0].value = v.ToString();
                InvokeFunc(kp.Value[i]);
                */
                ExeActionWithTrigger(kp.Value[i], v);

            }
        }
    }

    [ContextMenu("UnloadDlls")]
    private void UnloadDlls()
    {
        GC.Collect(); // collects all unused memory
        GC.WaitForPendingFinalizers(); // wait until GC has finished its work
        GC.Collect();

        actor.onReceiveEvent -= EventTrigger;
        IActor.onGlobalEvent -= GlobalEventTrigger;
    }
    private void OnDestroy()
    {
        UnloadDlls();
    }


    [ContextMenu("Load XML")]
    private void LoadXml()
    {
        string path = $"{Application.dataPath}/Datas/Actor/{xml.name}.xml";

        var settings = new XmlReaderSettings
        {
            // Allow processing of DTD
            DtdProcessing = DtdProcessing.Parse,
            // On older versions of .Net instead set 
            //ProhibitDtd = false,
            // But for security, prevent DOS attacks by limiting the total number of characters that can be expanded to something sane.
            MaxCharactersFromEntities = (long)1e7,
            // And for security, disable resolution of entities from external documents.
            XmlResolver = null,
        };
        XmlSerializer serializer = new XmlSerializer(typeof(IActor));
        StreamReader reader = new StreamReader(path);
        using (var xmlReader = XmlReader.Create(reader, settings))
        {
            //IActor deserialized = (IActor)serializer.Deserialize(reader.BaseStream);
            IActor deserialized = (IActor)serializer.Deserialize(xmlReader);
            actor = deserialized;
        }
        reader.Close();

        Debug.Log($"Loaded Actor: {actor.name}");
        Debug.Log($"Actor update time : {actor.updateTime}");
        Debug.Log($"Actor actions count : {actor.actions.Count}");


        //==========================[ 註冊 ]====================================

        for (int i = 0; i < actor.actions.Count; i++)
        {
            var _act = actor.actions[i];
            Debug.Log($"Action {actor.actions[i].name}");

            // ========================= [註冊事件] ============================                    
            if (_act.triggerType == IAction.TriggerType.UPDATE)
            {
                updateFuncs.Add(_act);
            }
            else if (_act.triggerType == IAction.TriggerType.ONSTART)
            {
                onStartFuncs.Add(_act);
            }
            else if (_act.triggerType == IAction.TriggerType.INPUT)
            {
                var _funcList = new List<IAction>();
                if (triggerActionsMap.TryGetValue(_act.triggerName, out _funcList))
                {
                    //掛載方法
                    _funcList.Add(_act);
                }
                //新增inpute 事件
                else
                {
                    triggerActionsMap.Add(_act.triggerName, new List<IAction>());
                    triggerActionsMap[_act.triggerName].Add(_act);
                }
            }
            else if (_act.triggerType == IAction.TriggerType.EVENT)
            {
                var _funcList = new List<IAction>();
                if (eventActionsMap.TryGetValue(_act.triggerName, out _funcList))
                {
                    //掛載方法
                    _funcList.Add(_act);
                }
                //新增inpute 事件
                else
                {
                    eventActionsMap.Add(_act.triggerName, new List<IAction>());
                    eventActionsMap[_act.triggerName].Add(_act);
                }
            }
            else if (_act.triggerType == IAction.TriggerType.GLOBAL_EVENT)
            {
                var _funcList = new List<IAction>();
                if (globalEventActionsMap.TryGetValue(_act.triggerName, out _funcList))
                {
                    //掛載方法
                    _funcList.Add(_act);
                }
                //新增inpute 事件
                else
                {
                    globalEventActionsMap.Add(_act.triggerName, new List<IAction>());
                    globalEventActionsMap[_act.triggerName].Add(_act);
                }
            }
            else
            {
                Debug.Log("<color=red> No Func type matched! </color>");
            }

            //載入action dll
            for (int j = 0; j < _act.dlls.Count; j++)
            {
                var _dll_item = _act.dlls[j];
                Debug.Log($"Dll name  {_dll_item.name}");
                Debug.Log($"Dll path  {_dll_item.path}");
                string _dll_path = $"{Application.streamingAssetsPath}/{_dll_item.path}{_dll_item.name}.dll";
                var dll = Assembly.LoadFile(_dll_path);

                //呼叫dll內方法
                for (int z = 0; z < actor.actions[i].dlls[j].functions.Count; z++)
                {
                    var func = actor.actions[i].dlls[j].functions[z];
                    string func_name = func.name;

                    //============================[ 類別實例 ]===================================
                    if (!typeInsctancesMap.ContainsKey(func.type))
                    {
                        var class1Type = dll.GetType(func.type);
                        //動態類別實例
                        dynamic c = Activator.CreateInstance(class1Type);
                        //MethodInfo mi = class1Type.GetMethod(func_name);
                        //mi.Invoke(c, new object[] { func.paras } );
                        typeInsctancesMap.Add(func.type, c);
                    }

                }


            }
        }

    }

    private bool ExeAction(IAction act)
    {
        bool hasExed = CheckCallAction(act);

        if (hasExed)
        {
            foreach (var _dll in act.dlls)
            {
                foreach (var _func in _dll.functions)
                {
                    InvokeFunc(_func);
                }
            }
        }
        return hasExed;
    }
    private bool ExeActionWithTrigger(IAction act, object triggerValue)
    {
        bool hasExed = CheckCallAction(act);

        if (hasExed)
        {
            foreach (var _dll in act.dlls)
            {
                foreach (var _func in _dll.functions)
                {
                    _func.paras[0].value = triggerValue.ToString();
                    InvokeFunc(_func);
                }
            }
        }
        return hasExed;
    }
    private bool ExeActionWithObjectsData(IAction act, object[] datas)
    {
        bool hasExed = CheckCallAction(act);

        if (hasExed)
        {
            foreach (var _dll in act.dlls)
            {
                foreach (var _func in _dll.functions)
                {
                    InvokeFunc(_func, datas);
                }
            }
        }
        return hasExed;
    }

    private bool CheckCallAction(IAction act)
    {
        //檢查順序
        if (currentAction != null && (Time.time > currentActionDueTime || (currentAction.interoperable && currentAction.prority <= act.prority)))
        {
            //Pass
            //Debug.Log($"<color=yellow>{act.name} Inturpte {currentAction.name} </color>");
        }
        else if (currentAction == null)
        {
            //Pass
        }
        else
        {
            return false;
        }

        //檢查冷卻時間
        float _nextTime;
        if (actCdMaps.TryGetValue(act, out _nextTime))
        {
            if (Time.time > _nextTime)
            {
                actCdMaps[act] = Time.time + act.cd;
                currentAction = act;
                currentActionDueTime = Time.time + act.duration;
                return true;
            }
            return false;
        }
        else
        {
            actCdMaps.Add(act, Time.time + act.cd);
            return true;
        }
    }

    private void InvokeFunc(Dll.Func func)
    {

        //取得類別實例
        var classInst = typeInsctancesMap[func.type];
        MethodInfo mi = classInst.GetType().GetMethod(func.name);
        if (func.paras != null)
            mi.Invoke(classInst, new object[] { func.paras });
        else
            mi.Invoke(classInst, null);
    }

    private void InvokeFunc(Dll.Func func, object[] args)
    {
        //取得類別實例
        var classInst = typeInsctancesMap[func.type];
        MethodInfo mi = classInst.GetType().GetMethod(func.name);
        mi.Invoke(classInst, new object[] { args });
    }

}
