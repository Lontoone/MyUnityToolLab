using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//管理遊玩畫面的手指操作
public class GamePlay_FingerControler : MonoBehaviour
{
    public static event Action<Vector2> OnTouched;
    //bool isDraggingUI = true;

    private void Update()
    {
#if UNITY_ANDROID
        if (Input.touchCount > 0)
        { 
            if (OnTouched != null)
            {
                OnTouched(Input.touches[0].position);
            }
            else
            {
                Debug.Log("ontouch is null");
            }
        }
        /*
        if (Input.touchCount > 0 && (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled))
        {
            isDraggingUI = true;
        }*/
#endif

#if UNITY_STANDALONE_WIN
        if (Input.GetMouseButton(0))
        {
            if (OnTouched != null)
            {
                Debug.Log(Input.mousePosition);
                OnTouched(Input.mousePosition);
            }
        }
#endif
    }
}
