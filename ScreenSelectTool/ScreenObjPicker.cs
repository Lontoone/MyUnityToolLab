using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using System.Linq;
using System.Runtime.Serialization;

//Screen Obj Picker
public class ScreenObjPicker : MonoBehaviour
{
    public static ScreenObjPicker screenObjPicker;
    private void Awake()
    {
        screenObjPicker = this;
    }


    ///<summary>選擇中的物件</summary>
    [SerializeField]
    public List<ISelectable> SelectingObjs = new List<ISelectable>();

    [SerializeField]
    Color selectRangeColor;//選擇框框的顏色

    [SerializeField]
    public HashSet<ISelectable> selectableObjects = new HashSet<ISelectable>(); //可選擇的物體
    public HashSet<ITargetable> targetableObjects = new HashSet<ITargetable>(); //可選擇的物體

    ITargetable currentTargetedObj;

    public event Action<RaycastHit> OnRMBDown;
    public event Action<RaycastHit> OnLMBDown;
    public event Action<RaycastHit> OnLMBHold;
    public event Action<RaycastHit> OnRMBHold;
    //public event Action<GameObject> OnBeingSelected; //被選擇時
    public event Action<GameObject> OnTargetSet; //設定攻擊目標
    public event Action<Vector3> OnMoveGoalSet; //設定移動點

    private void Start()
    {

    }
    private void Update()
    {
        //單點滑鼠右鍵
        //if (Input.GetMouseButton(1))
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000))
            {
                //Debug.DrawLine(ray.origin, hit.point);

                if (OnRMBDown != null)
                    OnRMBDown(hit);
                SetTarget(hit);
            }
        }

        //單選物件 (滑鼠左鍵)
        //if (Input.GetMouseButtonDown(0))
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000))
            {
                if (OnLMBDown != null)
                    OnLMBDown(hit);

                SelectObj(hit);
            }

        }

        //target選取結束
        if (Input.GetMouseButtonUp(1))
        {
            if (currentTargetedObj != null)
            {
                currentTargetedObj.BeingDischosen();
                currentTargetedObj = null;
            }
        }
    }
    ///<summary>單選取物件</summary>
    void SelectObj(RaycastHit hit)
    {
        SelectClear();
        ISelectable item = hit.transform.GetComponent<ISelectable>();
        if (item != null && item.Selectable)
        {
            if (!SelectingObjs.Exists(e => e == item))
            {
                SelectingObjs.Add(item);
                item.BeingChosen();
                Debug.Log("單選 " + item.Transform.name + " 共 " + SelectingObjs.Count);
            }
        }
    }

    ///<summary>滑鼠按下的第一個點</summary>
    Vector2 firstPos = new Vector2(0, 0);
    Vector2 previousPos = new Vector2(); //上一個frame的滑鼠位置
    private void OnGUI()
    {
        Vector2 nowPos = Event.current.mousePosition;
        //select start
        if (Input.GetMouseButtonDown(0))
        {
            firstPos = nowPos;

        }
        if (Input.GetMouseButton(0) && firstPos != Vector2.zero)
        {
            //畫出選擇框
            var rect = new Rect(firstPos, nowPos - firstPos);


            Texture2D _staticRectTexture = new Texture2D(1, 1);
            _staticRectTexture.SetPixel(0, 0, selectRangeColor);
            _staticRectTexture.Apply();

            GUIStyle _staticRectStyle = new GUIStyle();
            _staticRectStyle.normal.background = _staticRectTexture;

            GUI.Box(rect, GUIContent.none, _staticRectStyle);

            if (previousPos == nowPos) { return; }

            //選取物件
            RangeSelect();

        }

        //select end
        if (Input.GetMouseButtonUp(0)) { firstPos = Vector2.zero; }

        previousPos = nowPos;
    }


    void RangeSelect()
    {
        SelectClear();

        Debug.Log("Range Select" + selectableObjects.Count);

        if (selectableObjects == null)
        {
            Debug.Log(" is null ");
            return;
        }
        //foreach (ISelectable obj in selectableObjects)
        foreach (ISelectable obj in selectableObjects.ToList())
        {
            if (obj.Selectable && !SelectingObjs.Exists(e => e == obj))
            {
                Vector2 screen_pos;
                if (obj.Transform != null)
                {
                    screen_pos = Camera.main.WorldToScreenPoint(obj.Transform.position);
                    screen_pos.y = Camera.main.pixelHeight - screen_pos.y;
                    if (IsInsideRect(firstPos, Event.current.mousePosition, screen_pos))
                    {
                        SelectingObjs.Add(obj);
                        obj.BeingChosen();
                        Debug.Log("加入 " + obj.Transform.name + " 共 " + SelectingObjs.Count);
                    }
                }
                else
                {
                    selectableObjects.Remove(obj);
                }
            }


        }
    }
    void SetTarget(RaycastHit hit)
    {
        ITargetable item = hit.transform.GetComponent<ITargetable>();

        //Debug.Log("mouse 0  set target " + hit.transform.name);

        if (item != null && item.Targetable && SelectingObjs.Count > 0)
        {
            //設定攻擊目標
            if (OnTargetSet != null)
                OnTargetSet(hit.transform.gameObject);

            item.BeingChosen();
            currentTargetedObj = item;
        }
        else
        {
            //設定移動點
            if (OnMoveGoalSet != null)
                OnMoveGoalSet(hit.point);

            Debug.DrawLine(Camera.main.transform.position, hit.point);
        }
    }

    void SelectClear()
    {

        if (SelectingObjs.Count > 0)
        {
            foreach (ISelectable obj in SelectingObjs)
            {
                obj.BeingDischosen();
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

public interface ISelectable
{
    bool Selectable
    {
        get;
        set;
    }
    Transform Transform
    {
        get;
    }

    HashSet<ISelectable> Container
    {
        get;
    }

    void BeingChosen();
    void BeingDischosen();

}

public interface ITargetable
{
    bool Targetable
    {
        get;
        set;
    }
    Transform Transform
    {
        get;
    }
    HashSet<ITargetable> TargetableList
    {
        get;
    }
    void BeingChosen();
    void BeingDischosen();

}