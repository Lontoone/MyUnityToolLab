using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PhysicsControlListeners : MonoBehaviour
{
    public LayerMask ground_layer;
    public GameObject footPositon; //腳步位置
    //public Vector2 move_dir; //移動方向
    public bool isGrounded;
    Rigidbody2D rigidbody;
    public float touch_ground_radious =0.05f;
    bool last_frame_isGrounded;//上一次更新是否碰到地面
    public event Action eOnTouchGround;
    void Start()
    {
        rigidbody = gameObject.GetComponent<Rigidbody2D>();
        //產生腳步位置標記物件:
        GameObject temp = new GameObject();
        footPositon = Instantiate(temp, transform.position, Quaternion.identity, transform);
        footPositon.gameObject.name = "foot";
        Destroy(temp);

    }

    private void FixedUpdate()
    {
        //離開/碰地事件:
        if (last_frame_isGrounded == !isGrounded)
        {
            if (eOnTouchGround != null)
                eOnTouchGround();
            last_frame_isGrounded = isGrounded;
        }
        //碰地面偵測
        isGrounded = Physics2D.OverlapCircle(footPositon.transform.position, touch_ground_radious, ground_layer);

    }
}
