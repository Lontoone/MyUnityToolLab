using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//玩家用車控制
public class CartMoveControl : MonoBehaviour
{
    public bool useTerrainHeight = true;
    CartStopsManager stopsManager;
    public CartStopPoint currentStop, nextStop;
    public bool isStopping = false;
    public float speed = 5;
    public float rotateSpeed = 0.5f;
    float t = 0;
    void Start()
    {
        stopsManager = GameObject.FindObjectOfType<CartStopsManager>();
    }
    void FixedUpdate()
    {
        //往下一個點移動
        if (!isStopping)
        {
            //取的目前與下一個點的t
            t = (t + Time.deltaTime * speed) % 1;

            //取得移動點 
            Vector3 nextMovePoint = stopsManager.GetPointPos(currentStop, nextStop, t);
            if (useTerrainHeight)
            {
                nextMovePoint.y = Terrain.activeTerrain.SampleHeight(nextMovePoint);
            }

            //面向移動方向
            Vector3 dir = nextMovePoint - transform.position;
            //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir.normalized, transform.up), t);
            Vector3 cartRotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir.normalized, transform.up), Time.deltaTime * rotateSpeed).eulerAngles;

            //限制x軸轉動幅度
            cartRotation.x = Mathf.Clamp(cartRotation.x, -5, 5);
            transform.eulerAngles = cartRotation;


            //移動
            transform.position = nextMovePoint;
            //已到達下個點
            if (t >= 0.99f)
            {
                t = 0;
                //下一個目的點
                if (nextStop.doStop)
                {
                    isStopping = true;
                }
                currentStop = nextStop;
                nextStop = stopsManager.GetNextStop(currentStop);

            }

        }
    }

    //再發車
    public void Restart()
    {
        isStopping = false;
    }
}
