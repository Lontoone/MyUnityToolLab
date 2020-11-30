using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//管理遊玩畫面的手指操作
public class GamePlay_FingerControler : MonoBehaviour
{
    public static event Action<Vector2, bool> OnTouched;
    bool isDraggingUI = true;

    private void Update()
    {
        Debug.Log(Input.touchCount);
        if (Input.touchCount > 0)
        {
            TouchPhase touchPhase = Input.touches[0].phase;
            //判斷點到的UI還是主畫面?
            RaycastHit2D[] hits = TouchObjectFuntion.GetTouchedObj(0, touchPhase);
            foreach (RaycastHit2D hit in hits)
            {
                //Debug.Log(hit.transform.name);
                if (hit.transform.name == "MoveControl_Range")
                {
                    isDraggingUI = false;
                    break;
                }
            }
            Debug.Log(OnTouched == null);

            if (OnTouched != null)
            {
                //Debug.Log("On Touched" + Input.touches[0].position);
                OnTouched(Input.touches[0].position, isDraggingUI);
            }
            else
            {
                Debug.Log("ontouch is null");
            }
        }
        if (Input.touchCount > 0 && (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled))
        {
            isDraggingUI = true;
        }
    }
}
