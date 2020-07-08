using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//2D物理
public class My2Dphysic : MonoBehaviour
{
    public float gravity = 9.8f;
    public Vector2 velocity; // v=a*t
    public float mass = 1;
    public bool isEntity; //是否為可碰撞實體
    public Vector2 force = new Vector2(0, 0); //f=mass*gravity //在此為額外給予的力
    public float forceDrag = 0.1f;

    public LayerMask ground_layer;
    public LayerMask wall_layer;

    [HideInInspector]
    public GameObject footPositon; //腳步位置

    [HideInInspector]
    public bool is_near_wall = false;

    public Vector2 move_dir; //移動方向
    public bool isGrounded;

    float touch_ground_radious = 0.1f; //接觸判定範圍
    [HideInInspector]
    float pull_from_ground_radious = 0.05f;//陷入地面判定範圍

    public bool freezeRotation_z = false; //z軸旋轉鎖定?
    [Tooltip("影響到斜面下滑速度")]
    public float friction_self = 1; //自身摩擦力
    public float angular_acceleration = 1;//旋轉加速 =旋轉速度平方*半徑

    [HideInInspector]
    public My2Dcollider mCollider;

    private void Start()
    {
        mCollider = gameObject.GetComponent<My2Dcollider>();

        //產生腳步位置標記物件:
        GameObject temp = new GameObject();
        footPositon = Instantiate(temp, transform.position, Quaternion.identity, transform);
        footPositon.gameObject.name = "foot";


        Vector2 bounds_offset = new Vector2(gameObject.GetComponent<SpriteRenderer>().bounds.size.x * 0.5f, 0);

        Destroy(temp);

        if (mCollider != null)
        {
            mCollider.OnColliderEnter += Collider_enter;
            mCollider.OnColliderStay += Collider_stay;
            mCollider.OnColliderLeave += Collider_leave;
        }

    }
    private void OnDestroy()
    {
        if (mCollider != null)
        {
            mCollider.OnColliderEnter -= Collider_enter;
            mCollider.OnColliderStay -= Collider_stay;
            mCollider.OnColliderLeave -= Collider_leave;
        }
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(footPositon.transform.position, touch_ground_radious, ground_layer);

        //斜面處理:
        RaycastHit2D hit = Physics2D.Raycast(footPositon.transform.position, Vector2.down, 0.5f, ground_layer);
        if (hit.collider != null)
        {
            float slope_angle = Vector2.Angle(Vector2.up, hit.normal); //地面斜度

            Vector3 slope_down_force = Vector3.Cross(Vector3.up, (Vector3)hit.normal); //斜度對速度的加成
            float ground_frition = (hit.collider.sharedMaterial != null) ? hit.collider.sharedMaterial.friction : 1;//地面摩擦力

            //Debug.Log("地面斜度:" + slope_down_force);
            if (!freezeRotation_z)
            {
                gameObject.transform.Rotate(0, 0, slope_angle * angular_acceleration * Time.deltaTime);
                //TODO[]:因為碰撞判斷是圓形，方形物體不會那麼自然
            }

            //陸地(摩擦力)移動
            AddForce(new Vector2(-slope_down_force.z / ground_frition / friction_self / mass, 0) * Time.deltaTime);
        }


        //碰撞處理
        //在碰撞範圍內，給予被碰撞的物體v*m/m1

        //掉落:
        if (!isGrounded)
        {
            //往下加速度
            velocity.y -= gravity * Time.deltaTime;
        }
        else
        {
            //碰到地面時:
            if (velocity.y != 0)
            {
                //反作用力 f=ma
                float anitiForce = Mathf.Lerp(-velocity.y * mass / forceDrag, 0, Time.deltaTime); //TODO{}現在只會往上彈
                //if (anitiForce * 0.5f > gravity)
                if (anitiForce / mass > gravity)
                {
                    AddForce(-velocity.normalized * (anitiForce / mass - gravity));
                }
                velocity.y = 0;
            }

            //防止陷入地面下
            RaycastHit2D hit_in_ground = Physics2D.Raycast(footPositon.transform.position, Vector2.down, 0.25f, ground_layer);//?NOTE:distance太長會抓到地板的另一端
            if (hit_in_ground.collider != null)
            {
                transform.position = new Vector2(transform.position.x, hit_in_ground.point.y + pull_from_ground_radious * 2);//?NOTE:加的值太大會抖
                if (force.y < 0) { force.y = 0; }
            }
        }


        if (force != Vector2.zero)
        {
            //lerp to 0
            //forceDrag越大，越快消成0
            force = new Vector2(Mathf.Lerp(force.x, 0, forceDrag * Time.deltaTime), Mathf.Lerp(force.y, 0, forceDrag * Time.deltaTime));
        }

        //移動方向設定(分左右?)
        move_dir = (velocity + force).normalized;
        //if ((velocity.x + force.x) > 0) { move_dir = Vector2.right; }
        //else if ((velocity.x + force.x) < 0) { move_dir = Vector2.left; }

        //移動
        Vector2 _move = (Vector2)transform.position + (velocity + force) * Time.deltaTime;
        _move = checkNAN(_move);
        transform.position = _move;

        Wall_block();

    }

    public void AddForce(Vector2 _force)
    {
        force += _force;
    }


    //檢查碰牆
    void Wall_block()
    {
        //向移動的方向射ray，若ray打到牆壁則依面向的方向限制x軸
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (velocity + force).normalized, 0.5f, wall_layer);
        if (hit.collider == null)
        {
            is_near_wall = false;
            return;
        }
        is_near_wall = true;
        if (hit.normal.x < 0)
        { //牆在右:
            Vector2 _adjust_pos = transform.position;
            //物體x軸限制
            _adjust_pos.x = Mathf.Clamp(_adjust_pos.x, _adjust_pos.x, hit.point.x - 0.2f);
            transform.position = _adjust_pos;
        }
        else
        {
            //牆在左
            Vector2 _adjust_pos = transform.position;
            //物體x軸限制
            _adjust_pos.x = Mathf.Clamp(_adjust_pos.x, hit.point.x + 0.2f, _adjust_pos.x);
            transform.position = _adjust_pos;
        }

        AddForce(hit.normal * mass);

    }


    //碰撞 
    void Collider_enter(GameObject _collider)
    {
        My2Dphysic rigid = _collider.GetComponent<My2Dphysic>();
        //被撞到時:
        if (rigid != null && rigid.isEntity)
        {
            //能量轉移
            Vector2 _trans_force = (velocity) * mass;
            //Debug.Log(gameObject.name + " V: " + (rigid.velocity) + " enter " + _collider.name + " V: " + (velocity) + " ENTER FORCE:" + _trans_force);
            rigid.AddForce(_trans_force);
        }

    }
    void Collider_stay(GameObject _collider)
    {
        My2Dphysic rigid = _collider.GetComponent<My2Dphysic>();
        //把對方推開:
        if (rigid != null && rigid.isEntity)
        {
            Vector2 dir = transform.position - rigid.transform.position;
            //Vector2 dir = rigid.move_dir; //[bug]會滑掉

            rigid.AddForce(dir * mass * Time.deltaTime);
        }
    }

    void Collider_leave(GameObject _collider)
    {
        //collider Leave
    }



    private void OnDrawGizmos()
    {

        Gizmos.color = Color.green;
        //碰牆線
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (velocity + force).normalized, 5f, wall_layer);
        if (hit.collider != null)
        {
            Gizmos.DrawLine(transform.position, hit.point);
        }

        //TEST:
        /*
        if (footPositon != null)
        {
            Gizmos.color = Color.green;
            RaycastHit2D hit_in_ground = Physics2D.Raycast(footPositon.transform.position, Vector2.down, 0.25f, ground_layer);
            if (hit_in_ground.collider != null)
            {
                Gizmos.DrawLine(footPositon.transform.position, hit_in_ground.point);
            }
        }*/

    }


    private Vector2 checkNAN(Vector2 _v)
    {
        Vector2 _out = _v;
        if (float.IsNaN(_out.x) || float.IsInfinity(_out.x)) { _out.x = 0; }
        if (float.IsNaN(_out.y) || float.IsInfinity(_out.x)) { _out.y = 0; }
        return _out;
    }

}


//備註公式
/*  
    acceleration=force/mass;
    velocity=acceleration*Time.deltaTime;
        
*/