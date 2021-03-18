using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class FingerManager : MonoBehaviour
{
    public static event Action<Vector3, TouchPhase> OnTouched; //screen space
    public static event Action<Vector3, TouchPhase> OnTouchedWorld; // world space
    //bool isDraggingUI = true;

    private void Update()
    {
#if UNITY_IPHONE
        if (Input.touchCount > 0)
        {
            Debug.Log(Input.touches[0].position);
            if (OnTouched != null)
                OnTouched(Input.touches[0].position, Input.touches[0].phase);

            if (OnTouchedWorld != null)
                OnTouchedWorld(Camera.main.ScreenToWorldPoint(Input.touches[0].position), Input.touches[0].phase);

        }
#endif

#if UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Debug.Log(Input.touches[0].position);
            if (OnTouched != null)
                OnTouched(Input.touches[0].position, Input.touches[0].phase);

            if (OnTouchedWorld != null)
                OnTouchedWorld(Camera.main.ScreenToWorldPoint(Input.touches[0].position), Input.touches[0].phase);

        }
#endif

#if UNITY_STANDALONE_WIN
        if (Input.GetMouseButton(0))
        {
            
            //Debug.Log(Input.mousePosition);
            if (OnTouched != null)
                OnTouched(Input.mousePosition,TouchPhase.Began);
            
            if(OnTouchedWorld!=null )
                OnTouchedWorld(Camera.main.ScreenToWorldPoint(Input.mousePosition),TouchPhase.Began);
        }
#endif
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {

            //Debug.Log(Input.mousePosition);
            if (OnTouched != null)
                OnTouched(Input.mousePosition, TouchPhase.Began);

            if (OnTouchedWorld != null)
                OnTouchedWorld(Camera.main.ScreenToWorldPoint(Input.mousePosition), TouchPhase.Began);
        }
#endif
    }


}
