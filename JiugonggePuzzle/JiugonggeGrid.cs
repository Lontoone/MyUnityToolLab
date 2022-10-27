using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class JiugonggeGrid : MonoBehaviour
{
    JiugonggeCell selected_cell;
    public JiugonggeCell blankCell;

    SpriteRenderer sp;

    [SerializeField]
    [Header("Order Orientation: Horizontal")]
    //[Tooltip("會自動找child物件填入")]
    List<JiugonggeCell> cells = new List<JiugonggeCell>();
    [HideInInspector]
    public List<Vector2> originPos = new List<Vector2>();
    Camera camera;


    bool isHorizontal = false;

    public UnityEvent OnCorrect;
    public UnityEvent OnSwap;

    private void Start()
    {
        camera = Camera.main;
        sp = GetComponent<SpriteRenderer>();


        int i = 0;
        foreach (Transform child in transform)
        {
            JiugonggeCell _cell = child.GetComponent<JiugonggeCell>();
            _cell.index = i;
            originPos.Add(_cell.transform.position);
            cells.Add(_cell);
            i++;
        }
        //隨機
        //*四個都轉太困難了，只隨機轉兩個
        if (Random.Range(0, 100) < 50)
        {
            RandomSetup(new int[] { 0, 1, 4, 3 });
            RandomSetup(new int[] { 1, 2, 5, 6 });
        }
        else
        {
            RandomSetup(new int[] { 4, 5, 8, 7 });
            RandomSetup(new int[] { 3, 4, 7, 6 });
        }

    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 10, -1);
            if (hit.collider)
            {
                JiugonggeCell cell = hit.transform.GetComponent<JiugonggeCell>();
                if (cell != null)
                {

                    //檢查滑動方向:
                    if (IsNearBlankAndDir(cell))
                    {
                        //Debug.Log("IsNearBlank " + true);
                        selected_cell = cell;
                    }
                    else
                    {
                        //selected_cell = null;
                    }
                }
            }
        }
        else if (Input.GetMouseButtonUp(0) && selected_cell != null)
        {
            selected_cell.transform.position = originPos[selected_cell.index];
            selected_cell = null;

            //檢查答案
            CheckAnwser();
        }
        else if (Input.GetMouseButton(0))
        {
            if (selected_cell != null)
            {
                //拖拉
                Vector3 movePosition = selected_cell.transform.position;
                Vector2 _mousePosition = camera.ScreenToWorldPoint(Input.mousePosition);

                if (isHorizontal)
                {
                    movePosition.x = Mathf.Clamp(_mousePosition.x, sp.bounds.min.x, sp.bounds.max.x);

                }
                else
                {

                    //movePosition.y = _mousePosition.y;
                    movePosition.y = Mathf.Clamp(_mousePosition.y, sp.bounds.min.y, sp.bounds.max.y);

                }
                //Debug.Log(movePosition);



                selected_cell.transform.position = movePosition;

                //selected_cell.transform.position = camera.ScreenToWorldPoint(Input.mousePosition);


            }

        }
    }

    //對角的cell隨機設定
    private void RandomSetup(int[] origin2x2)
    {
        int[] res2x2 = new int[4];
        int random_offset = Random.Range(0, 4);
        for (int i = 0; i < 4; i++)
        {
            res2x2[i] = origin2x2[(random_offset + i) % 4];
            JiugonggeCell a = cells[origin2x2[i]];
            JiugonggeCell b = cells[res2x2[i]];
            SwapCell(a, b);
        }

    }

    //檢查是否在blank方塊旁邊
    bool IsNearBlankAndDir(JiugonggeCell _cell)
    {

        //垂直移動方向
        int top = blankCell.index - 3;
        int down = blankCell.index + 3;

        if ((top >= 0 && cells[top] == _cell) ||
            (down < cells.Count && cells[down] == _cell))
        {
            isHorizontal = false;
            return true;
        }

        //水平移動方向
        int left = blankCell.index - 1;
        int right = blankCell.index + 1;
        if ((left >= 0 && cells[left] == _cell) ||
            (right < cells.Count && cells[right] == _cell))
        {
            isHorizontal = true;
            return true;
        }



        return false;
    }

    void CheckAnwser()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].index != cells[i].ans)
            {
                //Debug.Log("Wrong " + i);
                return;
            }
        }

        //答對
        Debug.Log("Correct");
        //PointController.AddPoint(100, 2);
        if (OnCorrect != null)
        {
            OnCorrect.Invoke();
        }


    }

    public void SwapCell(JiugonggeCell a, JiugonggeCell b)
    {
        //Debug.Log("Swap");
        int a_index_temp = a.index;

        Vector3 tempPos = originPos[b.index];
        b.transform.position = originPos[a.index];
        a.transform.position = tempPos;

        a.index = b.index;
        b.index = a_index_temp;

        cells[a.index] = a;
        cells[b.index] = b;

        if (OnSwap != null)
            OnSwap.Invoke();
    }
}
