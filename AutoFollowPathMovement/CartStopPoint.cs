using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//車站
public class CartStopPoint : MonoBehaviour
{
    public Transform leftLerpHadle, rightLerpHandle;
    public bool doStop = false; //此站是否需要停車

    void OnDrawGizmos()
    {
        //畫左右把手
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, leftLerpHadle.position);
        Gizmos.DrawWireSphere(leftLerpHadle.position, 0.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, rightLerpHandle.position);
        Gizmos.DrawWireSphere(rightLerpHandle.position, 0.5f);
    }
}
