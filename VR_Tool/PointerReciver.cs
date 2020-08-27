using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
//接收玩家UI點擊    
public class PointerReciver : MonoBehaviour
{
    public UnityEvent pressedEvent;
    public UnityEvent hoverEvent;
    public UnityEvent leaveEvent;

    Image btnImg;
    Color originColor;
    public Color hoverColor, pressColor;


    private void Start()
    {
        Pointer.ePointer_down += OnPressDown;
        Pointer.ePointer_leave += OnPointerLeave;
        Pointer.ePointer_hover += OnHover;
        btnImg = gameObject.GetComponent<Image>();
      

        if (gameObject.GetComponent<Image>() != null)
        {
            originColor = gameObject.GetComponent<Image>().color;
        }
    }
    private void OnDestory()
    {
        Pointer.ePointer_down -= OnPressDown;
        Pointer.ePointer_leave -= OnPointerLeave;
        Pointer.ePointer_hover -= OnHover;
    }
    void OnHover(GameObject _obj, Vector3 cursorPos)
    {
        if (_obj == this.gameObject)
        {
            if (hoverEvent != null) hoverEvent.Invoke();//返回指標位置

            //hover特效
            if (btnImg != null)
            {
                btnImg.color = hoverColor;
            }
        }


    }

    void OnPressDown(GameObject _obj)
    {
        if (_obj == gameObject)
        {
            if (pressedEvent != null) pressedEvent.Invoke();

            //效果
            if (btnImg != null)
            {
                btnImg.color = pressColor;
            }

            Invoke("BacktoOrigineColor_afterClick", 0.25f);
        }
    }

    void OnPointerLeave(GameObject _obj)
    {
        if (_obj == gameObject)
        {
            Debug.Log(_obj.name + "leave");
            if (leaveEvent != null) leaveEvent.Invoke();

            //btn特效
            if (btnImg != null)
                btnImg.color = originColor;
        }
    }

    //按下去幾秒後再回去原本的顏色
    void BacktoOrigineColor_afterClick()
    {
        if (btnImg != null)
            btnImg.color = originColor;
    }
}
