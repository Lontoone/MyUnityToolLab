using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using System.Threading.Tasks;
//using System.Threading;

public class ActionController : MonoBehaviour
{
    public int maxActionCount = 5;//最多佇列工作
    public List<mAction> actionQueue = new List<mAction>(); //待執行工作
    public event Action eActionQueueCleared; //工作清單都完成時:

    [SerializeField]
    mAction currentAction;
    Coroutine cDoProcess;
    Coroutine cTimeOutCheck;
    WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

    //------------TEST----------------
    private void Start()
    {
        //await Task.Yield();
        Debug.Log("staty開始");


        Debug.Log("start結束");
    }

    IEnumerator DoProcess()
    {
        //執行排隊中的方法
        while (actionQueue.Count > 0)
        {
          
            //提取第一個執行
            currentAction = actionQueue[0];

            Debug.Log("Do action " + currentAction.description);
            actionQueue.RemoveAt(0);
            currentAction.is_in_gap_time_lock = true;

            //非一次性方法
            if (currentAction.isLoop)
            {
                float time_counter = 0;
                while (currentAction != null &&
                       currentAction.isLoop &&
                    time_counter < currentAction.duration)
                {
                    Debug.Log("Doing " + currentAction.description);
                    //currentAction.action();
                    if (currentAction.action != null)
                        currentAction.action.Invoke();
                    time_counter += Time.fixedDeltaTime;
                    yield return WaitForFixedUpdate;
                }
            }

            //一次性方法
            else
            {
                Debug.Log("Doing " + currentAction.description + " once");
                //currentAction.action();
                if (currentAction.action != null)
                    currentAction.action.Invoke();
                yield return new WaitForSeconds(currentAction.duration);
            }

            //各自進行冷卻
            Debug.Log("計算冷卻 " + currentAction.description);
            StartCoroutine(currentAction.ResetLock());

            //執行完
            Debug.Log("Action Done: " + currentAction.description);
            //currentAction = null;


            yield return null;
        }

        //清空時
        if (eActionQueueCleared != null)
            eActionQueueCleared();
        cDoProcess = null;
    }

    //檢查用
    private void FixedUpdate()
    {
        //檢查time Out
        if (cTimeOutCheck == null && actionQueue.Count > 0)
        {
            cTimeOutCheck = StartCoroutine(CheckTimeOut());
        }

        if (cDoProcess == null && actionQueue.Count > 0)
        {
            //cDoProcess = StartCoroutine(DoProcess());

        }
        if (actionQueue.Count == 0)
        {
            if (eActionQueueCleared != null)
                eActionQueueCleared();
        }


    }
    public void AddAction(mAction _newAct)
    {
        if (actionQueue.Count > maxActionCount)
        {
            return;
        }
     
        if (_newAct == null || _newAct.is_in_gap_time_lock)
        {
            Debug.Log(_newAct.description + " 還在冷卻");
            return;
        }//還在冷卻中

        Debug.Log("Add Action" + _newAct.description);
        _newAct.time_out_counter = _newAct.timeOut;

        //檢查是否可斷
        if (currentAction != null && cDoProcess != null &&
            _newAct.priority > currentAction.priority &&
            _newAct.force)
        {
            //先終止，加入後再重啟
            StopCoroutine(cDoProcess);
            currentAction.is_in_gap_time_lock = false;
            currentAction = null;
            cDoProcess = null;
        }

        actionQueue.Add(_newAct);
        actionQueue.Sort((a, b) => b.priority.CompareTo(a.priority));

        if (cDoProcess == null)
        {
            Debug.Log("啟動");
            cDoProcess = StartCoroutine(DoProcess());
            cTimeOutCheck = StartCoroutine(CheckTimeOut());
        }
    }

    //將排隊過久的刪除
    IEnumerator CheckTimeOut()
    {
        while (actionQueue.Count > 0)
        {
            actionQueue.RemoveAll(x => (x.time_out_counter -= 0.2f) < 0);
            yield return waitForSeconds;
        }
        cTimeOutCheck = null;
    }


    [System.Serializable]
    public class mAction
    {
        public mAction(Action _act, string _des, int _priority, bool _force, float _duration, float _timeOut)
        {
            //action = _act;
            description = _des;
            priority = _priority;
            force = _force;
            duration = _duration;
            timeOut = _timeOut;

        }
        public mAction(Action _act, string _des, int _priority, bool _force, float _duration, float _timeOut, bool _isLoop)
        {
            //action = _act;
            description = _des;
            priority = _priority;
            force = _force;
            duration = _duration;
            timeOut = _timeOut;
            isLoop = _isLoop;
        }
        public mAction(Action _act, string _des, int _priority, bool _force, float _duration, float _timeOut, bool _isLoop, float _gap_time)
        {
            //action = _act;
            description = _des;
            priority = _priority;
            force = _force;
            duration = _duration;
            timeOut = _timeOut;
            isLoop = _isLoop;
            gap_time = _gap_time;
        }

        public string description = ""; //描述 or 動作名稱
        public bool is_in_gap_time_lock = false;
        public float gap_time = 0.5f;
        //public Action action;
        public UnityEngine.Events.UnityEvent action;
        public int priority = 0;//優先度
        public bool force = false; //可以斷別人?
        public float duration; //執行時間
        public bool isLoop = true;
        public float timeOut; //排隊超過時間就刪除
        [HideInInspector]
        public float time_out_counter = 0; //計時用的，在進入佇列時更新
        public IEnumerator ResetLock()
        {
            Debug.Log(description + " 冷卻中");

            yield return new WaitForSeconds(gap_time);
            Debug.Log(description + " 冷卻結束");
            is_in_gap_time_lock = false;
        }

    }


}
