using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//自動基本AI
[RequireComponent(typeof(My2Dphysic))]
public class BasicAI : MonoBehaviour
{
    My2Dphysic rigid;
    public float speed = 5;
    public float max_move_distance = 5;//一次最大可移動距離
    public float max_move_height;//最高可移動點
    public float move_gap_time = 5;//設定目標點的間隔時間
    public float jump_force = 5; //跳躍力道
    //public bool jumpable = true; //可跳躍?
    public float reach_radious = 0.05f;//這範圍內碰到目標點就算抵達
    [HideInInspector]
    public bool reached_goal = false;//抵達?
    public LayerMask walkable_layer;
    public LayerMask attack_target_layer;

    //EVENT
    public event Action walk_event;
    public event Action idle_event;
    public event Action jump_event;

    public Collider2D enemy_detect_range;//偵測攻擊對象的collider
    
    [HideInInspector]
    public bool is_chasing_target = false;

    GameObject moveGoal;
    Coroutine _move_coro, _reset_move_coro;

    private void Start()
    {
        rigid = gameObject.GetComponent<My2Dphysic>();

        max_move_height = gameObject.GetComponent<SpriteRenderer>().bounds.size.y * 1.5f;//最大可移動距離

        //產生移動目標標記點:
        GameObject temp = new GameObject();
        moveGoal = Instantiate(temp, transform.position, Quaternion.identity);
        Destroy(temp);
        moveGoal.gameObject.name = gameObject.name + " move_goal";

        Set_move_goal();
    }

    private void FixedUpdate()
    {
        //左右翻轉:
        if (rigid.velocity.x > 0)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else if (rigid.velocity.x < 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }

        //檢查敵人是否在視線裡
        if (enemy_detect_range != null)
        {
            Detect_set_attackTarget();
        }

        //移動
        if (_move_coro == null)
        {
            _move_coro = StartCoroutine(Move_coro());

            if (!is_chasing_target)
            {
                if (_reset_move_coro != null)
                {
                    StopCoroutine(_reset_move_coro);
                }
                _reset_move_coro = StartCoroutine(Reset_move_coro());
            }
        }

    }

    protected virtual void Detect_set_attackTarget()
    {
        Collider2D[] colliders = new Collider2D[10];
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(attack_target_layer);
        int colliderCount = enemy_detect_range.OverlapCollider(contactFilter, colliders);
        if (colliderCount > 0)
        {
            moveGoal.transform.position = colliders[0].transform.position;
            is_chasing_target = true;
        }
        else
        {
            is_chasing_target = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (moveGoal != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, moveGoal.transform.position);
            Gizmos.DrawWireSphere(moveGoal.transform.position, reach_radious);
        }
    }
    //移動
    IEnumerator Move_coro()
    {
        //到達目標?
        while (Vector2.Distance(transform.position, moveGoal.transform.position) > reach_radious)
        {
            //是否碰到牆?
            if (rigid.is_near_wall)
            {
                //JUMP
                //if (jumpable)
                if (jump_force > 0)
                {
                    rigid.AddForce(Vector2.up * jump_force );
                    if (jump_event != null) { jump_event(); Debug.Log("<color=green>JUMP</color>"); }
                    yield return new WaitForFixedUpdate();
                }
            }

            //平地移動
            Vector2 _dir = moveGoal.transform.position - transform.position;
            rigid.velocity.x = _dir.normalized.x * speed;

            if (walk_event != null) { walk_event(); Debug.Log("<color=green>WALK</color>"); }
            yield return new WaitForFixedUpdate();
        }

        //到達:移動結束
        rigid.velocity.x = 0;
        reached_goal = true;
        if (idle_event != null)
        {
            idle_event(); Debug.Log("<color=green>IDLE</color>");
        }

        //等待後重新找目標
        yield return new WaitForSeconds(move_gap_time);

        if (!is_chasing_target)
            Set_move_goal();

        _move_coro = null;
    }

    //逾時刷新移動目標
    IEnumerator Reset_move_coro()
    {
        yield return new WaitForSeconds(move_gap_time * 2.5f);
        Debug.Log(gameObject.name + " <color=red>逾時刷新</color>");
        //逾時重置
        Set_move_goal();
        StopCoroutine(Move_coro());
        _move_coro = null;
        _reset_move_coro = null;
    }

    //設定移動點目標
    void Set_move_goal()
    {
        //隨機找範圍內一點
        Vector2 _rand_point = new Vector2(transform.position.x + UnityEngine.Random.Range(-max_move_distance, max_move_distance), transform.position.y);
        moveGoal.transform.position = _rand_point;

        bool is_in_ground = Physics2D.OverlapCircle(moveGoal.transform.position, 0.05f, walkable_layer);

        if (is_in_ground)
        {
            //若點陷入地面: 往上移動
            int _m_count = 0;
            while (Physics2D.OverlapCircle(moveGoal.transform.position, 0.05f, walkable_layer))
            {
                moveGoal.transform.position = new Vector2(moveGoal.transform.position.x,
                     moveGoal.transform.position.y + 0.05f);
                if (_m_count++ > 100) break;
            }
        }

        else
        {
            int _m_count = 0;
            //若點浮在空中=>往下移動
            while (!Physics2D.OverlapCircle(moveGoal.transform.position, 0.05f))
            {
                moveGoal.transform.position = new Vector2(moveGoal.transform.position.x,
                     moveGoal.transform.position.y - 0.05f);
                if (_m_count++ > 100) break;
            }
        }


        //若頂端的點高度>或<可跳躍高度=>取消
        if (Mathf.Abs(moveGoal.transform.position.y - transform.position.y) > max_move_height)
        {
            //找不到點=>預設不移動
            moveGoal.transform.position = transform.position;
            reached_goal = true;
        }
        else
        {
            //找到可移動的點:
            reached_goal = false;
        }

    }
}
