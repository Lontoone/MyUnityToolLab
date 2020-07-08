using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[RequireComponent(typeof(Collider2D))]
public class My2Dcollider : MonoBehaviour
{
    public LayerMask entity_layer;

    public event Action<GameObject> OnColliderEnter;
    public event Action<GameObject> OnColliderStay;
    public event Action<GameObject> OnColliderLeave;

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
    [SerializeField]
    public List<Collider2D> current_colliders = new List<Collider2D>();//目前的collider

    private void Update()
    {
        if (hit_check_coro == null)
        {
            hit_check_coro = StartCoroutine(Hit_Check_coro());
            Collider2D myCollider = gameObject.GetComponent<Collider2D>();

            Collider2D[] colliders = new Collider2D[10];

            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(entity_layer);
            int colliderCount = myCollider.OverlapCollider(contactFilter, colliders);

            //檢查離開的 (紀錄有但這次沒有的)
            for (int i = 0; i < current_colliders.Count; i++)
            {
                if (!Array.Exists(colliders, x => x == current_colliders[i]))
                {
                    Debug.Log("LEAVE " + current_colliders[i].gameObject.name);
                    OnColliderLeave(current_colliders[i].gameObject);
                    current_colliders.RemoveAt(i);
                }
            }

            for (int i = 0; i < colliderCount; i++)
            {
                if (colliders[i].gameObject != gameObject)
                {
                    if (!current_colliders.Contains(colliders[i]) && OnColliderEnter != null)
                    {
                        OnColliderEnter(colliders[i].gameObject);
                        current_colliders.Add(colliders[i]);
                    }

                    else if (OnColliderStay != null)
                    {
                        OnColliderStay(colliders[i].gameObject);
                    }
                }
            }
        }

    }

    IEnumerator Hit_Check_coro()
    {
        yield return new WaitForSeconds(hit_interval);
        hit_check_coro = null;
    }


}