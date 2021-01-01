using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{

    public List<MovePoint> movePoints;
    public int maxSteps = 30;
    public float stepWidth = 2, stepHeight = 0.5f;
    public float jumpHeight = 5;
    public float fallHeight = 10; //可落下距離
    public Transform goal;
    public GameObject debug_dot;
    public bool shortest = false;

    Vector3 previous_split_normal;

    //debug
    public LineRenderer lineRenderer;

    private void Start()
    {
        //TEST
        //int res = FindPath(-1, transform.position, transform.position, movePoints);
        int res = FindPath(0, transform.position, Vector3.zero, movePoints);
        Debug.Log(res);
        if (res == -1)
        {   //失敗
            Debug.Log("失敗");
            movePoints.Clear();

        }
        else if (res == 0)
        {

            lineRenderer.positionCount = movePoints.Count;
            for (int i = 0; i < movePoints.Count; i++)
            {
                lineRenderer.SetPosition(i, movePoints[i].point);

                GameObject _d = Instantiate(debug_dot, movePoints[i].point, Quaternion.identity);
                _d.name = i.ToString();
            }
        }
    }
    int FindPath(int count, Vector3 start, Vector3 previous, List<MovePoint> result)
    {

        //超過步數限制 終止
        if (count > maxSteps)
        {
            Debug.Log("STEP OUT");
            return -1;
        }

        //在一步範圍內 =>到達
        else if (Vector3.Distance(start, goal.position) < stepWidth)
        {
            MovePoint final = new MovePoint();
            final.point = goal.position;
            result.Add(final);

            Debug.Log("到達!");
            return 0;
        }


        //判斷下個點
        else
        {
            Vector3 dir = goal.position - start;
            RaycastHit hit;

            if (Physics.Raycast(start, dir.normalized, out hit, stepWidth))
            {
                //遇到障礙物  檢查是否可以跳過
                Vector3 _next = new Vector3();
                if (CheckCanJump(hit.point, start, out _next))
                {
                    Debug.Log("可跳過 " + hit.transform.name);

                    if (FindPath(count + 1, _next, start, result) != -1)
                    {
                        result.Add(new MovePoint(start));
                        return count;
                    }
                    else
                    {
                        return -1;
                    }
                }

                //不可跳躍障礙 => 分裂路線
                else if (hit.normal != previous_split_normal)
                {
                    Debug.Log("不可跳過 " + hit.transform.name + " " + count);
                    //Split
                    previous_split_normal = hit.normal;

                    var s = Split(previous, start, hit);

                    List<MovePoint> res1 = new List<MovePoint>();
                    List<MovePoint> res2 = new List<MovePoint>();

                    //res1.Add(new MovePoint(start));
                    //res2.Add(new MovePoint(start));

                    if (s.Count == 2)
                    {
                        Debug.DrawLine(start, s[0], Color.magenta, 15);
                        Debug.DrawLine(start, s[1], Color.magenta, 15);
                        int r1 = FindPath(count + 1, s[0], start, res1);
                        Debug.Log("split 1 result: " + r1);
                        //若不取最短路線，則找到第一條可行的就好
                        if (!shortest && r1 != -1)
                        {
                            result.AddRange(res1);
                            return count;
                        }
                        else
                        {

                            int r2 = FindPath(count + 1, s[1], start, res2);
                            Debug.Log("split 2 result: " + r2);
                            //Debug.Log(res1.Count + " vs " + res2.Count);

                            //兩條路都可走則取最短
                            if (r1 != -1 && r2 != -1)
                            {
                                if (res1.Count > res2.Count)
                                    result.AddRange(res2);
                                else
                                    result.AddRange(res1);
                            }
                            //走r2
                            else if (r1 == -1 && r2 != -1)
                                result.AddRange(res2);
                            //走r1
                            else if (r2 == -1 && r1 != -1)
                                result.AddRange(res1);
                            //兩條都不通
                            else if (r2 == -1 && r1 == -1)
                            {
                                return -1;
                            }
                        }

                        result.Add(new MovePoint(start));
                        return count;
                    }

                    //只有一條路
                    else if (s.Count == 1)
                    {
                        if (FindPath(count + 1, s[0], previous, res1) != -1)
                        {
                            result.AddRange(res1);
                            result.Add(new MovePoint(start));
                            return count;
                        }
                    }
                    //找不到路
                    else
                    {
                        Debug.Log("split to 0 => return -1 " + count);
                        return -1;
                    }

                }
                else
                {
                    //照上一次移動方向移動
                    Vector3 _move = (start - previous).normalized * stepWidth + start;
                    if (FindPath(count + 1, _move, start, result) != -1)
                    {
                        result.Add(new MovePoint(start));
                        return count;
                    }
                }

            }

            //無障礙物
            else
            {
                previous_split_normal = Vector3.zero;
                
                //檢查下個點是否浮空
                Vector3 _next = new Vector3();
                Vector3 nextStop = start + dir.normalized * stepWidth;

                //跳躍
                if (CheckCanJump(Vector3.zero, start, out _next))
                {

                    if (FindPath(count + 1, _next, start, result) != -1)
                    {
                        result.Add(new MovePoint(start));
                        return count;
                    }
                    else
                    {
                        return -1;
                    }
                }

                //可直行
                else if (FindPath(count + 1, nextStop, start, result) != -1)
                {
                    //Debug.Log("直行");
                    result.Add(new MovePoint(start));
                    return count;
                }
                else
                {
                    return -1;
                }

            }

        }
        return -1;
    }


    bool CheckCanJump(Vector3 hit, Vector3 start, out Vector3 nextPos)
    {
        //往上跳
        if (hit != Vector3.zero)
        {
            //打到牆壁

            //最高跳點
            Vector3 max_jump_point = hit;
            max_jump_point.y += jumpHeight;
            /*
            Vector3 toJumpTop = max_jump_point - start;
            //斜邊長
            float rayLength = Mathf.Pow(
                    Mathf.Pow(Vector3.Distance(hit, start), 2) + Mathf.Pow(max_jump_point.y, 2),
                    0.5f);

            //最高跳點有牆壁
            //Debug.DrawRay(start, toJumpTop.normalized, Color.yellow, 5);
            //if (Physics.Raycast(start, toJumpTop.normalized, rayLength))
            */
            if (Physics.CheckSphere(max_jump_point, 0.5f))//用ray測的hit point會剛好卡在牆裡面 半徑數值先亂抓的
            {
                nextPos = Vector3.zero; //null 失敗
                Debug.Log("無法跳躍的牆");
                return false;
            }
            //最高跳點無牆壁
            else
            {
                nextPos = max_jump_point; //TODO:改成跳定點?
                return true;
            }
        }

        //往下跳
        else
        {
            Vector3 next = start + (goal.position - start).normalized * stepWidth; //下個點(浮空)

            RaycastHit fall_hit;
            //在範圍內可落地
            if (Physics.Raycast(next, Vector3.down, out fall_hit, stepHeight)) //腳步高度內不算跳
            {
                nextPos = fall_hit.point;
                nextPos.y += stepHeight;
                return false;
            }
            else if (Physics.Raycast(next, Vector3.down, out fall_hit, fallHeight))
            {
                nextPos = fall_hit.point;
                nextPos.y += stepHeight;
                return true;
            }
            //地板落差太高
            else
            {
                nextPos = Vector3.zero;
                return false;
            }


        }
    }


    List<Vector3> Split(Vector3 from, Vector3 start, RaycastHit hit)
    {

        List<Vector3> _res = new List<Vector3>();

        //hit normal 轉90度與-90度的方向
        Vector3 p90_dir = new Vector3(hit.normal.z, hit.normal.y, -hit.normal.x);
        Vector3 p90 = p90_dir * stepWidth + start;
        Vector3 n90_dir = new Vector3(-hit.normal.z, hit.normal.y, hit.normal.x);
        Vector3 n90 = n90_dir * stepWidth + start;
        //Vector3 p90 = GetRotated_Pos(90, hit.point + hit.normal * stepWidth, hit.point);
        //Vector3 n90 = GetRotated_Pos(-90, hit.point + hit.normal * stepWidth, hit.point);

        //Debug.Log(p90 + " " + n90);
        RaycastHit p90_hit, n90_hit;
        Debug.DrawLine(start, from, Color.red, 15);
        //Debug.DrawLine(start, n90_dir, Color.red, 15);

        //檢查兩條分裂的線段有沒有與來的路線重疊，有就不採用
        if (!CheckLinesOverLapped(hit.point, p90, start, from))
        {

            if (Physics.Raycast(start, p90_dir, out p90_hit, stepWidth))
            {
                _res.Add(p90_hit.point);
                //_res.Add((p90_hit.point - start) * 0.5f + start);
            }
            else
            {
                _res.Add(p90);
            }
        }
        if (!CheckLinesOverLapped(hit.point, n90, start, from))
        {

            if (Physics.Raycast(start, n90_dir, out n90_hit, stepWidth))
            {
                _res.Add(n90_hit.point);
                //_res.Add((n90_hit.point - start) * 0.5f + start);
            }
            else
            {
                _res.Add(n90);
            }
        }

        Debug.Log("split to " + _res.Count);
        return _res;
    }

    bool CheckLinesOverLapped(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
    {
        float angle = Vector2.Dot((a1 - a2).normalized, (b1 - b2).normalized);
        //Debug.Log(angle);
        if (angle < -0.9f || angle == 0) { return true; }

        //以b1,b2線段法線為投影軸
        Vector2 _line = (b2 - b1).normalized;
        Vector2 _normal = new Vector2(_line.y, -_line.x).normalized;

        //投影x軸 [小、大]
        float[] ax = { 0, 0 };
        float[] bx = { 0, 0 };
        ax[0] = Vector2.Dot(new Vector2(a1.x, a1.z), _line);
        ax[1] = Vector2.Dot(new Vector2(a2.x, a2.z), _line);
        Swap(ref ax);

        bx[0] = Vector2.Dot(new Vector2(b1.x, b1.z), _line);
        bx[1] = Vector2.Dot(new Vector2(b2.x, b2.z), _line);
        Swap(ref bx);

        //投影y軸
        float[] ay = { 0, 0 };
        float[] by = { 0, 0 };
        ay[0] = Vector2.Dot(new Vector2(a1.x, a1.z), _normal);
        ay[1] = Vector2.Dot(new Vector2(a2.x, a2.z), _normal);
        Swap(ref ay);

        by[0] = Vector2.Dot(new Vector2(b1.x, b1.z), _normal);
        by[1] = Vector2.Dot(new Vector2(b2.x, b2.z), _normal);
        Swap(ref by);

        /*
        if ((ax[0] <= bx[1] && ax[0] >= bx[0]) ||
            (ax[1] <= bx[1] && ax[1] >= bx[0]))
        {
            if ((ay[0] <= by[1] && ay[0] >= by[0]) ||
                (ay[1] <= by[1] && ay[1] >= by[0]))
            {
                Debug.Log("重疊");
                return true;
            }
        }*/

        if (ax[0] < bx[1] && bx[1] < ax[1] &&
            ay[0] < by[1] && by[1] < ay[1])
        {
            return true;
        }
        Debug.Log(a1 + " " + a2 + " " + b1 + " " + b2);

        return false;
    }

    void Swap(ref float[] arry)
    {
        if (arry[0] > arry[1])
        {
            float temp = arry[0];
            arry[0] = arry[1];
            arry[1] = temp;
        }
    }

    //旋轉矩陣 弧度angle
    Vector3 GetRotated_Pos(float angle, Vector3 currentPos, Vector3 center)
    {

        //var new_x = (currentPos.x * Mathf.Cos(angle)) - (currentPos.y * Mathf.Sin(angle));
        //var new_y = (currentPos.x * Mathf.Sin(angle)) + (currentPos.y * Mathf.Cos(angle));
        //return new Vector2(new_x, new_y);
        //return Quaternion.Euler(0, 0, angle) * (currentPos - center) + center;
        return Quaternion.Euler(0, angle, 0) * (currentPos - center) + center;

        //繞y軸旋轉
        //var new_x=(currentPos.x * Mathf.Cos(angle)) + (currentPos.z * Mathf.Sin(angle));
    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.blue;
        Vector3 jumpTop = transform.position;
        jumpTop.y += jumpHeight;
        Gizmos.DrawLine(transform.position, jumpTop);

        Gizmos.color = Color.red;
        Vector3 step_dis = transform.position;
        step_dis.x += stepWidth;
        Gizmos.DrawLine(transform.position, step_dis);

    }

    [System.Serializable]
    public class MovePoint
    {
        public MovePoint() { }
        public MovePoint(Vector3 _p)
        {
            point = _p;
        }

        public Vector3 point;
        public bool doJump = false;
    }
}
