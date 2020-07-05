using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[RequireComponent(typeof(Collider2D))]
public class My2Dcollider : MonoBehaviour
{
    //[HideInInspector]
    //public Vector2 size = new Vector2(1, 1);
    //[HideInInspector]
    //public Vector2 offset = new Vector2(0, 0);

    public LayerMask entity_layer;

    public event Action<GameObject> OnColliderEnter;
    public event Action<GameObject> OnColliderStay;
    public event Action<GameObject> OnColliderLeave;
    bool isStaying = false;
    GameObject last_Collide_obj;

    //bool can_hit = true;
    float hit_interval = 0.2f;
    Coroutine hit_check_coro;


    private void OnBecameInvisible()
    {
        hit_interval = 1;
    }
    private void OnBecameVisible()
    {
        hit_interval = 0.05f;
    }

    private void Update()
    {
        //RaycastHit2D hit_collider = Physics2D.BoxCast((Vector2)transform.position + offset, size, 0, Vector2.up, 0.5f, entity_layer);
        if (hit_check_coro == null)
        {
            hit_check_coro = StartCoroutine(Hit_Check_coro());
            Collider2D myCollider = gameObject.GetComponent<Collider2D>();
            Collider2D[] colliders = new Collider2D[10];
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(entity_layer);
            int colliderCount = myCollider.OverlapCollider(contactFilter, colliders);
            for (int i = 0; i < colliderCount; i++)
            {
                //Debug.Log(colliders[i].gameObject.name);

                if (colliders[i].gameObject != gameObject)
                {
                    if (isStaying == false && OnColliderEnter != null)
                    {
                        OnColliderEnter(colliders[i].gameObject);
                        isStaying = true;
                    }
                    else if (OnColliderStay != null)
                    {
                        OnColliderStay(colliders[i].gameObject);
                        last_Collide_obj = colliders[i].gameObject;
                    }


                }
                if (colliders[i] == last_Collide_obj)
                {
                    Debug.Log("LEAVE");
                    isStaying = false;
                    if (OnColliderLeave != null)
                    {
                        OnColliderLeave(last_Collide_obj);
                    }
                }

            }
            if (colliderCount <= 0 && isStaying)
            {
                isStaying = false;
            }

        }

    }

    IEnumerator Hit_Check_coro()
    {

        yield return new WaitForSeconds(hit_interval);
        hit_check_coro = null;
    }


}