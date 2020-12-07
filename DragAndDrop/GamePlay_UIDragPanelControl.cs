using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class GamePlay_UIDragPanelControl : MonoBehaviour
{
    public GamePlay_UI_btn graggingObj;
    public float gragSpeed = 15;
    public float swap_gap=10;
    public static event Action<List<GamePlay_UI_btn>> eEndDrag; //結束拖拉

    [SerializeField]
    List<GamePlay_UI_btn> uiLayout_list = new List<GamePlay_UI_btn>(); //展示每個角色狀態的UI
    [SerializeField]
    List<Vector2> uiLayoutOriginPos_list = new List<Vector2>(); //原本的位置(一開始固定，別改)

    //bool isOnDiscard = false;
    //丟棄區圖示
    public Sprite[] discard_backpack_imgs;
    public Image discard_backpack;

    private void Start()
    {
        //取得子物件+排列
        int i = 0;
        foreach (Transform child in transform)
        {
            uiLayout_list.Add(child.GetComponent<GamePlay_UI_btn>());
            uiLayoutOriginPos_list.Add(child.transform.position);

            //下排UI產生
            if (uiLayout_list.Count > i)
            {
                GamePlay_UI_btn _ui_btn = uiLayout_list[i].GetComponent<GamePlay_UI_btn>();

                _ui_btn.ui_sort_order = i;
            }
            else
            {
                //跳過
                uiLayout_list[i].gameObject.SetActive(false);
                uiLayout_list[i].GetComponent<GamePlay_UI_btn>().ui_sort_order = 999 + i;
                i++;
                continue;
            }

            //掛上event監聽
            EventTrigger trigger = child.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            EventTrigger.Entry entry_dragging = new EventTrigger.Entry();
            EventTrigger.Entry entry_end = new EventTrigger.Entry();

            //entry.eventID = EventTriggerType.PointerDown;
            entry.eventID = EventTriggerType.InitializePotentialDrag;
            entry_dragging.eventID = EventTriggerType.Drag;
            entry_end.eventID = EventTriggerType.EndDrag;

            entry.callback.AddListener((eventdata) => { btn_OnSelected(); });
            entry_dragging.callback.AddListener((eventdata) => { SortWhenDragging(); });
            entry_end.callback.AddListener((eventdata) => { OnEndDrag(); });

            trigger.triggers.Add(entry);
            trigger.triggers.Add(entry_dragging);
            trigger.triggers.Add(entry_end);

            child.name = i++.ToString();
        }

        GamePlay_FingerControler.OnTouched += Gragging;

    }
    private void OnDestroy()
    {
        GamePlay_FingerControler.OnTouched -= Gragging;
    }

    public void Gragging(Vector2 _goal, bool isDragging)
    {
        if (!isDragging || graggingObj == null) { return; }

        Vector3 _move = new Vector3(
                        graggingObj.transform.position.x,
                        Mathf.Lerp(graggingObj.transform.position.y, _goal.y, gragSpeed * Time.deltaTime),
                        0);
        graggingObj.transform.position = _move;
    }

    public void btn_OnSelected()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
            return; ;
        GamePlay_UI_btn selected = EventSystem.current.currentSelectedGameObject.GetComponent<GamePlay_UI_btn>();
        if (selected == null) { return; }
        graggingObj = selected;
    }

    //拖拉時排序:
    public void SortWhenDragging()
    {
        foreach (GamePlay_UI_btn _ui in uiLayout_list)
        {
            if (_ui == graggingObj || graggingObj == null) { continue; }

            //如果正在拉的物件接近自己=> 和他交換固定位置
            if (Vector2.Distance(graggingObj.transform.position, _ui.transform.position) < swap_gap)
            {
                //Debug.Log(graggingObj.name + " swap " + _ui.name + " : " + Vector2.Distance(graggingObj.transform.position, _ui.transform.position));
                SwapOriginPos(graggingObj, _ui);
                break;
            }


        }
    }

    //移動時交換位置:
    void SwapOriginPos(GamePlay_UI_btn tra1, GamePlay_UI_btn tra2)
    {

        tra1.transform.position = uiLayoutOriginPos_list[tra2.ui_sort_order];
        tra2.transform.position = uiLayoutOriginPos_list[tra1.ui_sort_order];

        string _temp = tra1.ui_sort_order.ToString();
        tra1.name = tra2.ui_sort_order.ToString();
        tra2.name = _temp;

        int _temp_order = tra1.ui_sort_order;
        tra1.ui_sort_order = tra2.ui_sort_order;
        tra2.ui_sort_order = _temp_order;

    }

    //結束Drag:定位
    void OnEndDrag()
    {
        graggingObj.transform.position = uiLayoutOriginPos_list[graggingObj.ui_sort_order];
        uiLayout_list.Sort((x, y) => x.ui_sort_order.CompareTo(y.ui_sort_order));

        graggingObj = null;

        //結束
        if (eEndDrag != null)
            eEndDrag(uiLayout_list);

    }


}
