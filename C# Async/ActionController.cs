using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;

public class ActionController : MonoBehaviour
{

    public List<mAction> actionQueue = new List<mAction>(); //待執行工作
    public Task processQueueingTask; //處理queue中
    public event Action eActionQueueCleared; //工作清單都完成時:

    Task _currentTask;
    CancellationTokenSource _currentTask_cts = new CancellationTokenSource();
    CancellationTokenSource process_cts = new CancellationTokenSource();

    mAction _act_idle, _act_walk, _act_dash;

    [SerializeField]
    bool isProcessig = false;
    //------------TEST----------------
    private async void Start()
    {
        await Task.Yield();
        Debug.Log("staty開始");

        _act_idle = new ActionController.mAction(
                      Idle,
                      "Idle",
                      0,
                      false,
                      1,
                      10
                  );
        _act_walk = new ActionController.mAction(
                 Move,
                 "Move",
                 1,
                 false,
                 1,
                 10
             );

        _act_dash = new ActionController.mAction(
           Dash,
           "Dash",
           5,
           true,
           1,
           10
       );



        Debug.Log("start結束");
    }
    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            AddAction(_act_idle);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            AddAction(_act_walk);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            AddAction(_act_dash);
        }


        if (!isProcessig && actionQueue.Count > 0)
        {
            StartProcess();
        }
        await Task.Yield();
    }

    private void OnDestroy()
    {
        //不清掉他會繼續跑
        actionQueue.Clear();
    }


    async void StartProcess()
    {
        isProcessig = true;
        process_cts = new CancellationTokenSource();
        var result = await Task.Run(() => ProcessActionQueue(process_cts.Token));
        Debug.Log(result);

        if (result == 0)
        {
            isProcessig = false;
        }
        //被終止
        if (result < 0)
        {
            isProcessig = false;
        }

    }
    //新增動作
    public async void AddAction(mAction _newAct)
    {
        //if (actionQueue.Count > 0)
        if (isProcessig)
        {
            Debug.Log("加入" + _newAct.description);
            AddActionToQueue(_newAct);
        }
        else
        {
            Debug.Log("啟動並加入" + _newAct.description);

            AddActionToQueue(_newAct);

            //processQueueingTask = Task.Run(() => ProcessActionQueue(process_cts.Token));
            StartProcess();


        }
        await Task.Yield();
    }

    async void FuncTest()
    {
        await Task.Delay(5000);
    }

    //處理任務
    private async Task<int> ProcessActionQueue(CancellationToken token)
    {
        _currentTask_cts = new CancellationTokenSource();
        while (actionQueue.Count > 0)
        {
            if (token.IsCancellationRequested)
            {
                Debug.Log("process 接收終止!" + actionQueue[0].description);
                _currentTask_cts.Cancel();
                Debug.Log(_currentTask.IsCanceled);
                return -1;
            }

            //執行排序最前面的
            Debug.Log("Do Action " + actionQueue[0].description);

            _currentTask = Task.Run(actionQueue[0].action, _currentTask_cts.Token);
            await _currentTask;
            await Task.Delay((int)actionQueue[0].duration * 1000);//for debug

            //執行完後移除
            Debug.Log("Action done :" + actionQueue[0].description);
            actionQueue.RemoveAt(0);


            //actionQueue.Sort((a, b) => b.priority.CompareTo(a.priority));
        }

        //工作列清空
        if (eActionQueueCleared != null)
            eActionQueueCleared();

        return 0;
    }

    async void AddActionToQueue(mAction _newAction)
    {
        //檢查優先度是否比正在執行的action高? 且可斷目前action
        if (actionQueue.Count > 0 &&
            _newAction.priority > actionQueue[0].priority &&
            _newAction.force)
        {
            //中斷目前action，執行新action
            actionQueue.Add(_newAction);
            StopAction(actionQueue[0]);
            await Task.Yield();
            return;
        }

        //加入queue
        actionQueue.Add(_newAction);
        actionQueue.Sort((a, b) => b.priority.CompareTo(a.priority));

        await Task.Yield();
    }


    //終止Action
    public async void StopAction(mAction _old)
    {

        Debug.Log("終止" + _old.description);

        process_cts.Cancel();
        actionQueue.Remove(_old);
        actionQueue.Sort((a, b) => b.priority.CompareTo(a.priority));

        await Task.Yield();
    }


    [System.Serializable]
    public class mAction
    {
        public mAction(Action _act, string _des, int _priority, bool _force, float _duration, float _timeOut)
        {
            action = _act;
            description = _des;
            priority = _priority;
            force = _force;
            duration = _duration;
            timeOut = _timeOut;
        }
        public string description = ""; //描述 or 動作名稱
        public Action action;
        public int priority = 0;//優先度
        public bool force = false; //可以斷別人?
        public float duration; //執行時間
        public float timeOut; //排隊超過時間就刪除? 沒有實際用到

        public CancellationTokenSource cts = new CancellationTokenSource();
    }


    //-------TEST function----------------
    void Idle()
    {
        Debug.Log("Idle.......");
    }

    void Move()
    {
        Debug.Log("Moving......");
    }
    void Dash()
    {
        Debug.Log("Dashing......");
    }
}
