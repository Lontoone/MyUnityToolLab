using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
//using System.Threading.Tasks;
//using System.Threading;

public class ActionController : MonoBehaviour
{
    public int maxActionCount = 5;//最多佇列工作
    public List<mAction> actionQueue = new List<mAction>(); //待執行工作

    public event Action eActionQueueCleared; //工作清單都完成時:
    public event Action eDestoried;

    [SerializeField]
    mAction currentAction;
    Coroutine cDoProcess;
    Coroutine cTimeOutCheck;
    WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);
    public bool allowDuplicate = true;//允許重複動作?

    public bool stopWhenInvisiable = true;
    bool isVisiable = true;

    private void OnBecameInvisible()
    {
        if (stopWhenInvisiable)
        {
            this.StopAllCoroutines();
            currentAction = null;
            actionQueue.Clear();

            isVisiable = false;
        }
    }
    private void OnBecameVisible()
    {
        isVisiable = true;
    }

    private void OnDestroy()
    {
        this.StopAllCoroutines();
        actionQueue.Clear();

        if (eDestoried != null)
            eDestoried();
    }

    IEnumerator DoProcess()
    {
        //執行排隊中的方法
        while (actionQueue.Count > 0)
        {
            int next_index = !actionQueue.Any() ? -1 :
                            actionQueue
                            .Select((value, index) => new { Value = value.priority, Index = index })
                            .Aggregate((a, b) => (a.Value > b.Value) ? a : b)
                            .Index;
            Debug.Log("next" + next_index);
            /*
            //刪掉還在冷卻的
            if (actionQueue[next_index].is_in_gap_time_lock && actionQueue.Count > 1)
            {
                actionQueue.RemoveAt(next_index);
                yield return WaitForFixedUpdate;
                //continue;
            }*/

            //提取第一個執行
            currentAction = actionQueue[next_index];

            Debug.Log("Do action " + currentAction.description);
            //actionQueue.RemoveAt(0);
            actionQueue.RemoveAt(next_index);
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
            /*
            //各自進行冷卻
            Debug.Log("計算冷卻 " + currentAction.description);
            StartCoroutine(currentAction.ResetLock());
            */

            //執行完
            Debug.Log("Action Done: " + currentAction.description);
            currentAction.callbackEvent?.Invoke();

            //currentAction = null;


            yield return WaitForFixedUpdate;
        }

        //清空時
        if (eActionQueueCleared != null)
            eActionQueueCleared();
        cDoProcess = null;
    }

    //檢查用
    /*
    private void FixedUpdate()
    {
       
        if (!isVisiable) { return; }
        if (cDoProcess == null && actionQueue.Count > 0)
        {
            cDoProcess = StartCoroutine(DoProcess());
        }

        if (actionQueue.Count == 0)
        {
            //Debug.Log(gameObject.name + "all clear");
            if (eActionQueueCleared != null)
                eActionQueueCleared();
        }
    }

    */
    public void AddAction(mAction _newAct)
    {
        if (!allowDuplicate)
        {
            //if (_newAct == null || _newAct.is_in_gap_time_lock || actionQueue.Contains(_newAct))
            if (_newAct == null || _newAct.CheckCoolTime() || actionQueue.Contains(_newAct))
            {
                //Debug.Log(_newAct.description + " 還在冷卻");
                return;
            }//還在冷卻中
        }


        Debug.Log("Add Action" + _newAct.description);
        _newAct.time_out_counter = _newAct.timeOut;

        //檢查是否可斷
        if (currentAction != null && cDoProcess != null &&
            _newAct.priority > currentAction.priority &&
            _newAct.force)
        {
            Debug.Log(_newAct.description + " 斷 " + currentAction.description);
            //做Callback:
            currentAction.callbackEvent?.Invoke();
            //先終止，加入後再重啟
            StopCoroutine(cDoProcess);
            currentAction.is_in_gap_time_lock = false;
            currentAction = null;
            cDoProcess = null;
        }

        if (actionQueue.Count > maxActionCount)
        {
            return;
        }

        actionQueue.Add(_newAct);

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

    public void ClearCurrent()
    {
        currentAction = null;
    }

    [System.Serializable]
    public class mAction
    {
        public mAction() { }
        /// <param name="_des">描述.</param>
        /// <param name="_priority">優先度</param>
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
        public UnityEngine.Events.UnityEvent callbackEvent;
        public int priority = 0;//優先度
        public bool force = false; //可以斷別人?
        public float duration; //執行時間
        public bool isLoop = true;
        public float timeOut; //排隊超過時間就刪除
        double called_time; //使用的now毫秒

        [HideInInspector]
        public float time_out_counter = 0; //計時用的，在進入佇列時更新
        public IEnumerator ResetLock()
        {
            Debug.Log(description + " 冷卻中");

            yield return new WaitForSeconds(gap_time);
            Debug.Log(description + " 冷卻結束");
            is_in_gap_time_lock = false;
        }

        public bool CheckCoolTime()
        {
            //檢查目前時間是否超過冷卻時間
            if (DateTime.Now.Millisecond - called_time > gap_time)
            {
                is_in_gap_time_lock = false;
            }
            else
            {
                is_in_gap_time_lock = true;
            }
            return is_in_gap_time_lock;
        }

    }


}
