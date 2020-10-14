using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class CartStopsManager : MonoBehaviour
{
    public List<CartStopPoint> stopPoints = new List<CartStopPoint>();
    public int smoothness = 5;
    public bool closeLoop = false;
    public LineRenderer lineRenderer;
    void Start()
    {
        GetStopsInOrder();
        lineRenderer = gameObject.GetComponent<LineRenderer>();
    }
    //從圖層取得車站
    public void GetStopsInOrder()
    {
        stopPoints.Clear();
        stopPoints = GetComponentsInChildren<CartStopPoint>().ToList();
        //依照圖層排序
        stopPoints.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        if (closeLoop)
            stopPoints.Add(stopPoints[0]);
    }

    //取得移動曲線上的點
    public Vector3 GetPointPos(CartStopPoint current, CartStopPoint end, float t)
    {

        return current.transform.position * Mathf.Pow((1 - t), 3) +
            3 * current.rightLerpHandle.position * t * Mathf.Pow((1 - t), 2) +
            3 * end.leftLerpHadle.position * t * t * (1 - t) +
            end.transform.position * t * t * t;
    }

    //取得下個站點
    public CartStopPoint GetNextStop(CartStopPoint current)
    {
        int currentIndex = stopPoints.FindIndex(x => x == current);
        return stopPoints[(currentIndex + 1) % stopPoints.Count];
    }

    void OnDrawGizmos()
    {
        //畫出點與點之間的連線
        GetStopsInOrder();
        lineRenderer.positionCount = (stopPoints.Count - 1) * smoothness;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            float t = (float)i % smoothness / (float)smoothness;
            int pointIndex = i / smoothness;
            Vector3 point = GetPointPos(stopPoints[pointIndex], stopPoints[pointIndex + 1], t);
            lineRenderer.SetPosition(i, point);
        }
    }


}
