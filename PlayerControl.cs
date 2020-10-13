using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//玩家控制
public class PlayerControl : MonoBehaviour
{
    public float speed = 5;
    public float jumpForce = 10;
    bool isFalling = false;
    //[HideInInspector]
    //public bool isFacing_left = true;
    Animator animator;
    Rigidbody2D rigid;
    PhysicsControlListeners listeners;
    HitableObj hitable;
    PlayerAttackControl playerAttack;
    SpriteRenderer sp;

    int jump_count = 0; //跳躍次數 (for 2段跳)
    private void Start()
    {
        rigid = gameObject.GetComponent<Rigidbody2D>();
        listeners = gameObject.GetComponent<PhysicsControlListeners>();
        animator = gameObject.GetComponent<Animator>();
        playerAttack = gameObject.GetComponent<PlayerAttackControl>();
        sp = gameObject.GetComponent<SpriteRenderer>();

        hitable = gameObject.GetComponent<HitableObj>();
        if (hitable != null)
        {
            hitable.Die_event += Die;
            hitable.gotHit_event += Hurt;
        }

        listeners.eOnTouchGround += ResetJumpCount;
    }
    private void OnDestroy()
    {
        if (hitable != null)
        {
            hitable.Die_event -= Die;
            hitable.gotHit_event -= Hurt;
        }

        listeners.eOnTouchGround -= ResetJumpCount;
    }
    private void Update()
    {
        //不移動條件:
        if (playerAttack.input_s != "")
        {
            //rigid.velocity=Vector2.zero;
            return;
        }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt")) { return; }


        //rigid.velocity.x = Input.GetAxis("Horizontal") * speed;
        rigid.velocity = new Vector2(Input.GetAxis("Horizontal") * speed, rigid.velocity.y);
        //跳躍
        if (Input.GetKeyDown(KeyCode.Space) && (listeners.isGrounded || jump_count < 1))
        {
            jump_count++;
            Debug.Log("Jump");
            animator.Play("Jump_Start"); ;
            //rigid.AddForce(Vector2.up * jumpForce);
            //rigid.velocity+=new Vector2(0,jumpForce);
            rigid.velocity = new Vector2(rigid.velocity.x, jumpForce);
        }
    }

    private void FixedUpdate()
    {
        //不移動條件:
        if (playerAttack.input_s != "") { return; }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt")) { return; }

        //左右翻轉:
        if (rigid.velocity.x > 0)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            //sp.flipX = false;
        }
        else if (rigid.velocity.x < 0)
        {
            //sp.flipX = true;
            transform.eulerAngles = new Vector3(0, 180, 0);

        }

        //動畫判定:
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("fixed")) { return; }//不可斷動畫

        if (Mathf.Abs(rigid.velocity.y) > jumpForce * 0.05f) //跳躍
        {
            if (rigid.velocity.y > 0)
            {
                animator.Play("Jump_Start");
            }
            else
            {
                animator.Play("Jump_Fall");
            }

        }
        else if (Mathf.Abs(rigid.velocity.x) > 1f && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Jump")) //移動
        {
            animator.Play("Walk");

        }
        else //IDLE
        {
            animator.Play("Idle");
        }
    }

    void Hurt()
    {
        if (hitable.isHitable)
        {
            //被擊退
            rigid.AddForce(new Vector2(transform.forward.z, 0) * 2.5f);
            animator.Play("Hurt");
        }
    }
    void Die()
    {
        Debug.Log("玩家死亡");
    }

    void ResetJumpCount()
    {
        jump_count = 0;
    }
}
