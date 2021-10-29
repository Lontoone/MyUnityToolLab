using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PointController : MonoBehaviour
{
    public int current_lv = 0;
    public UnityEngine.UI.Text point_text;
    public static event Action<int> OnPointAdd;
    public static int[] points = { 0, 0, 0 };
    public void ResetPoint()
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = 0;
        }
    }

    public static int total
    {
        get
        {
            return points[0] + points[1] + points[2];
        }
    }

    public static void AddPoint(int point, int Levelindex)
    {
        points[Levelindex] += point;

        if (OnPointAdd != null)
        {
            OnPointAdd(Levelindex);
        }
    }


    private void Start()
    {
        OnPointAdd += UpdatePointText;
    }
    private void OnDestroy()
    {
        OnPointAdd -= UpdatePointText;
    }

    void UpdatePointText(int lv)
    {
        if (point_text != null)
            point_text.text = points[lv].ToString();

    }

    public void GetPointToText(UnityEngine.UI.Text text)
    {
        text.text = points[current_lv].ToString();
    }
}
