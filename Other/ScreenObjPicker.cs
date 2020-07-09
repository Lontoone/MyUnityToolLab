using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

//Screen Obj Picker
public class ScreenObjPicker : MonoBehaviour
{
    public static ScreenObjPicker screenObjPicker;
    private void Awake()
    {
        screenObjPicker = this;
    }

    ///<summary>可行走的物件</summary>
    public LayerMask WalkableLayers;
    public LayerMask SelectableLayers;

    ///<summary>選擇中的物件</summary>
    [SerializeField]
    List<ControlableObj> SelectingObjs = new List<ControlableObj>();

    ///<summary>選中的物體可執行的行為</summary>
    public event Action DoableEvent;


    [SerializeField]
    Color selectRangeColor;//選擇框框的顏色

    private void Update()
    {
        //選擇物件
        if (Input.GetMouseButtonDown(0))
        {
            SelectObj();
        }
        if (SelectingObjs.Count > 0 && Input.GetMouseButtonDown(1))
        {
            foreach (ControlableObj obj in SelectingObjs)
            {
                //選擇路徑
                if (obj.workMode == ControlableObj.WorkMode.Move)
                {
                    SelectPath();
                }
                else if (obj.workMode == ControlableObj.WorkMode.Attack)
                {

                }
            }
        }

        //偵測執行選擇物體的可執行動作
        if (DoableEvent != null)
        {
            DoableEvent();
        }
        //else { Debug.Log("ACTION沒東西"); }
    }
    ///<summary>單選取物件</summary>
    void SelectObj()
    {
        Vector2 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        //選擇物體
        if (Physics.Raycast(ray, out hit, 10, SelectableLayers))
        {
            ControlableObj item = hit.collider.GetComponent<ControlableObj>();
            if (item != null &&
                hit.collider.GetComponent<NavMeshAgent>() != null)
            {
                if (!SelectingObjs.Exists(e => e == item))
                {
                    //SelectingObj = hit.collider.GetComponent<ControlableObj>();
                    SelectingObjs.Add(item);
                    item.GotChosen();
                    //agent = hit.collider.GetComponent<NavMeshAgent>();
                    //Debug.Log(SelectingObj.name);
                }
            }
        }
       
    }

    ///<summary>移動</summary>
    void SelectPath()
    {
        Vector2 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100, WalkableLayers))
        {
           
            foreach (ControlableObj obj in SelectingObjs)
            {
                obj.movePoint = hit.point;
                obj.GetComponent<NavMeshAgent>().SetDestination(hit.point);
                //TODO:特效
            }
        }
    }

    ///<summary>滑鼠按下的第一個點</summary>
    Vector2 firstPos = new Vector2(0, 0);
    private void OnGUI()
    {
        if (Input.GetMouseButtonDown(0))
        {
            firstPos = Event.current.mousePosition;
        }
        if (Input.GetMouseButton(0) && firstPos != Vector2.zero)
        {
            //畫出選擇框
            var rect = new Rect(firstPos, Event.current.mousePosition - firstPos);

            Texture2D _staticRectTexture = new Texture2D(1, 1);
            _staticRectTexture.SetPixel(0, 0, selectRangeColor);
            _staticRectTexture.Apply();

            GUIStyle _staticRectStyle = new GUIStyle();
            _staticRectStyle.normal.background = _staticRectTexture;

            GUI.Box(rect, GUIContent.none, _staticRectStyle);

            //選取物件
            RangeSelect();
        }

        if (Input.GetMouseButtonUp(0)) { firstPos = Vector2.zero; }
    }

    
    void RangeSelect()
    {
        //SelectingObjs.Clear();
        SelectClear();
        foreach (ControlableObj obj in GameObject.FindObjectsOfType<ControlableObj>())
        {
            if (!SelectingObjs.Exists(e => e == obj))
            {
                Vector2 screen_pos;
                screen_pos = Camera.main.WorldToScreenPoint(obj.transform.position);
                //screen_pos = RectTransformUtility.WorldToScreenPoint(Camera.main, obj.transform.position);
                screen_pos.y = Camera.main.pixelHeight - screen_pos.y;
                if (IsInsideRect(firstPos, Event.current.mousePosition, screen_pos))
                {
                    SelectingObjs.Add(obj);
                    obj.GotChosen();
                    //Debug.Log(firstPos + " " + Event.current.mousePosition + " " + obj.gameObject.name + " " + screen_pos);
                }
            }

        }
    }

    void SelectClear()
    {
        if (SelectingObjs.Count > 0)
        {
            foreach (ControlableObj obj in SelectingObjs)
            {
                obj.DeChosen();
            }
            SelectingObjs.Clear();
        }
    }

    bool IsInsideRect(Vector2 _start, Vector2 _end, Vector2 _point)
    {

        //判斷拉方塊方向
        if (_end.x > _start.x)
        {//往右
            if (_end.y > _start.y)
            {//往下(end的y大)
                if (_point.x > _start.x && _point.x < _end.x
                    && _point.y < _end.y && _point.y > _start.y) { return true; }
            }
            else
            {//往上(end的y小)
                if (_point.x > _start.x && _point.x < _end.x
                    && _point.y > _end.y && _point.y < _start.y) { return true; }
            }
        }
        //往左
        else
        {
            if (_end.y > _start.y)
            {//往下
                if (_point.x < _start.x && _point.x > _end.x
                    && _point.y < _end.y && _point.y > _start.y) { return true; }
            }
            else
            {//往上
                if (_point.x < _start.x && _point.x > _end.x
                    && _point.y > _end.y && _point.y < _start.y) { return true; }
            }
        }
        return false;
    }
}
