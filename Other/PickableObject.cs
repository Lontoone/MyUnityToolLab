using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class PickableObject : MonoBehaviour
{

    [HideInInspector]
    public Vector3 movePoint;

    protected Rigidbody rigidbody;
    protected Animator animator;
    protected NavMeshAgent navMeshAgent;

    public virtual void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody>();
        animator = gameObject.GetComponent<Animator>();
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
    }

    public virtual void FixedUpdate()
    {
        //移動中
        if (Vector3.Distance(movePoint, transform.position) > 1 && movePoint != Vector3.zero)
        { Moving(); }

        //IDLE中
        else
        {
            Idling();
        }

    }
  

  
    ///<summary>移動中</summary>
    public virtual void Moving() { }
    ///<summary>Idle中</summary>
    public virtual void Idling() { }

    ///<summary>選中時執行</summary>
    public void GotChosen()
    {
        ScreenObjPicker.screenObjPicker.DoableEvent += DoableActions;
    }

    ///<summary>取消選取時執行</summary>
    public void DeChosen()
    {
        ScreenObjPicker.screenObjPicker.DoableEvent -= DoableActions;
    }

    ///<summary>可選擇使用的動作</summary>
    protected virtual void DoableActions()
    {
        //override....
    }

}
