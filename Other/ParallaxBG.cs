using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParallaxBG : MonoBehaviour
{
    public float parallaxSpeed;
    Vector3 camStartPos;

    [SerializeField]
    bool showGizmo = false;

    public float xLimit_start = 0;
    public float xLimit_end = 0;
    public Camera camera;

    //在視線內才執行
    [SerializeField]
    float inSight_radious = 30;

    bool ChcekIsInsight()
    {
        if (Vector2.Distance(camera.transform.position, transform.position) < inSight_radious)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Start()
    {
        camStartPos = camera.transform.position;
    }

    private void Update()
    {
        //取得camera移動距離
        Vector3 camera_move = camera.transform.position - camStartPos;
        camStartPos = camera.transform.position;
        
        if (!ChcekIsInsight()) { return; }
        if (xLimit_end != 0 && camera.transform.position.x > xLimit_end) { return; }
        if (xLimit_start != 0 && camera.transform.position.x < xLimit_start) { return; }


        transform.position += camera_move * parallaxSpeed;
    }

    /*
        private void FixedUpdate() {
            Color color= Color.black;
            Vector2 _start=transform.position;
            _start.x-=inSight_distance/2;
            Debug.DrawLine(_start,transform.position,color,Time.fixedTime);

        }*/

    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(transform.position, inSight_radious);

            Gizmos.color = Color.blue;

            Vector2 limit_start = transform.position;
            if (xLimit_start != 0)
                limit_start.x = xLimit_start;

            Vector2 limit_end = transform.position;
            if (xLimit_end != 0)
                limit_end.x = xLimit_end;
            Gizmos.DrawLine(limit_start, limit_end);
        }
    }


}
