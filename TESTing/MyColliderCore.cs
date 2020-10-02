using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
//碰撞核心
public class MyColliderCore : MonoBehaviour
{
    public List<ColliderGroup> colliderGroups = new List<ColliderGroup>();
    public float max_group_bounds = 10;
    bool canCheck = true;
    private void FixedUpdate()
    {
        canCheck = !canCheck;

        //TEST
        foreach (ColliderGroup group in colliderGroups)
            GroupCollision(group);
    }
    private void Start()
    {
        //MyCollider.eMoved += GroupCollision;
        MyCollider.eMoved += GroupCheck;

        colliderGroups = SetUpAllGroups(max_group_bounds);
        Debug.Log("組數" + colliderGroups.Count);
    }
    private void OnDestroy()
    {
        //MyCollider.eMoved -= GroupCollision;
        MyCollider.eMoved -= GroupCheck;
    }

    public List<ColliderGroup> SetUpAllGroups(float max_size) //設定所有group
    {
        colliderGroups.Clear();
        List<ColliderGroup> groups = new List<ColliderGroup>();
        List<ColliderGroup> groups_tmp = new List<ColliderGroup>();
        //TODO:分群演算法
        MyCollider[] allColliders = FindObjectsOfType<MyCollider>();
        //一開始所有物件都自己一組
        foreach (MyCollider c in allColliders)
        {
            ColliderGroup tmp = new ColliderGroup();
            tmp.colliders.Add(c);
            tmp.UpdateCenter();
            c.belongGroup = tmp;
            groups_tmp.Add(tmp);
        }

        //===開始合併===
        groups.Add(groups_tmp[0]);
        groups_tmp.RemoveAt(0);
        for (int i = 0; i < groups.Count; i++)
        {
            for (int j = 0; j < groups_tmp.Count; j++)
            {
                if (groups[i].colliders.Exists(a => a == groups_tmp[j].colliders[0])) { continue; }
                if (CheckIsInsideBoundsCircle(groups[i].center, max_group_bounds, groups_tmp[j].colliders[0]))
                {
                    groups[i] = MergeGroup(groups[i], groups_tmp[j].colliders[0]);
                    groups[i].UpdateCenter();
                    //groups_tmp.RemoveAt(j);
                }
                else if (i == 0)
                {
                    groups.Add(groups_tmp[j]);
                }


            }
        }
        //groups.AddRange(groups_tmp);

        groups.Distinct();

        return groups;
    }

    ColliderGroup MergeGroup(ColliderGroup groupA, ColliderGroup groupB)//B融進A
    {
        ColliderGroup newGroup = new ColliderGroup();
        newGroup.colliders = groupA.colliders.Concat(groupB.colliders).ToList();
        return newGroup;
    }
    ColliderGroup MergeGroup(ColliderGroup groupA, MyCollider collider)//B融進A
    {
        ColliderGroup newGroup = new ColliderGroup();
        newGroup.colliders = groupA.colliders;
        newGroup.colliders.Add(collider);
        return newGroup;
    }

    //檢查碰撞
    public bool CheckCollision(MyCollider colliderA, MyCollider colliderB)
    {
        for (int i = 0; i < colliderA.vertices.Count; i++)
        {
            Vector2 current_normal = colliderA.normals[i];
            float[] AMinMax = colliderA.GetMaxMinDot(current_normal, colliderA.vertices, colliderA.transform);
            float[] BMinMax = colliderB.GetMaxMinDot(current_normal, colliderB.vertices, colliderB.transform);

            Debug.DrawLine(new Vector2(AMinMax[0], 0), new Vector2(AMinMax[1], 0), Color.black, 1f);
            Debug.DrawLine(new Vector2(BMinMax[0], 0), new Vector2(BMinMax[1], 0), Color.white, 1f);

            if (AMinMax[0] > BMinMax[1] || BMinMax[0] > AMinMax[1])
            {
                //Debug.Log(colliderA.transform.name + " to " + colliderB.transform.name + "<color=green> SAFE!</color>");

                return false;
            }
        }
        for (int i = 0; i < colliderB.vertices.Count; i++)
        {
            Vector2 current_normal = colliderB.normals[i];
            float[] AMinMax = colliderA.GetMaxMinDot(current_normal, colliderA.vertices, colliderA.transform);
            float[] BMinMax = colliderB.GetMaxMinDot(current_normal, colliderB.vertices, colliderB.transform);
            Debug.DrawLine(new Vector2(AMinMax[0], 0), new Vector2(AMinMax[1], 0), Color.red, 1f);
            Debug.DrawLine(new Vector2(BMinMax[0], 0), new Vector2(BMinMax[1], 0), Color.green, 1f);

            if (AMinMax[0] > BMinMax[1] || BMinMax[0] > AMinMax[1])
            {
                Debug.Log(colliderB.transform.name + " to " + colliderA.transform.name + "<color=green> SAFE!</color>");

                return false;
            }
        }
        return true;

    }

    //檢查組內碰撞
    public void GroupCollision(ColliderGroup group)
    {
        //檢查該組有沒有移動過
        if (!group.CheckHasMoved())
        {
            return;
        }

        int group_colliders_count = group.colliders.Count;
        foreach (MyCollider collider in group.colliders)
        {
            for (int i = 0; i < group_colliders_count - 1; i++)
            {
                for (int j = i + 1; j < group_colliders_count; j++)
                {
                    int j_index = j % group_colliders_count;
                    bool isColliding = CheckCollision(group.colliders[i], group.colliders[j_index]);
                    //TODO:碰撞處理
                    if (isColliding)
                    {
                        //互相登記對方
                        group.colliders[i].AddCollider(group.colliders[j_index]);
                        group.colliders[j_index].AddCollider(group.colliders[i]);
                    }
                    else
                    {
                        //TODO:檢查離開
                        group.colliders[i].RemoveCollider(group.colliders[j_index]);
                        group.colliders[j_index].RemoveCollider(group.colliders[i]);
                    }
                }
            }
        }
    }
    //檢查移動後的物體是否還在該group範圍內
    public void GroupCheck(MyCollider collider)
    {
        //沒超過group範圍=>更新該group center位置
        if (CheckIsInsideBoundsCircle(collider.belongGroup.center, max_group_bounds, collider))
        {
            //collider.belongGroup.UpdateCenter();
        }
        else
        {
            //colliderGroups = SetUpAllGroups(max_group_bounds);

            //如果與原center太遠=> 找最近的group
            for (int i = 0; i < colliderGroups.Count; i++)
            {
                if (CheckIsInsideBoundsCircle(colliderGroups[i].center, max_group_bounds, collider))
                {
                    ColliderGroup oldGroup = collider.belongGroup;
                    //找到就轉換
                    TransferGroup(collider.belongGroup, colliderGroups[i], collider);

                    return;
                }
            }

            //找不到=>自立一個
            ColliderGroup newGroup = new ColliderGroup();
            TransferGroup(collider.belongGroup, newGroup, collider);

        }
    }

    public ColliderGroup FindNearestGroup(MyCollider collider)
    {
        ColliderGroup newGroup = new ColliderGroup();
        //foreach Groups 找center離collider最近的group
        return newGroup;
    }

    public bool CheckIsInsideBoundsCircle(Vector2 center, float radious, MyCollider c)
    {
        //AABB檢測
        // Rect1
        float minX1 = center.x - radious;
        float maxX1 = center.x + radious;
        float minY1 = center.y - radious;
        float maxY1 = center.y + radious;
        // Rect2
        float minX2 = c.spr.bounds.center.x - c.spr.bounds.extents.x;
        float maxX2 = c.spr.bounds.center.x + c.spr.bounds.extents.x;
        float minY2 = c.spr.bounds.center.y - c.spr.bounds.extents.y;
        float maxY2 = c.spr.bounds.center.y + c.spr.bounds.extents.y;

        if (maxX1 > minX2 && maxX2 > minX1 &&
            maxY1 > minY2 && maxY2 > minY1)
        {
            return true;
        }
        else
            return false;
    }

    ///<summary>若from組轉移後剩0個collider會自動刪掉</summary>
    public void TransferGroup(ColliderGroup from, ColliderGroup to, MyCollider collider)
    {
        if (!colliderGroups.Contains(to))
        {
            colliderGroups.Add(to);
        }

        from.colliders.Remove(collider);
        collider.belongGroup = to;
        to.colliders.Add(collider);

        to.UpdateCenter();

        if (from.colliders.Count < 1)
        {
            colliderGroups.Remove(from);
        }
        else
        {
            from.UpdateCenter();
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < colliderGroups.Count; i++)
        {
            foreach (MyCollider c in colliderGroups[i].colliders)
            {
                Gizmos.DrawLine(colliderGroups[i].center, c.transform.position);
            }
            Gizmos.DrawWireSphere(colliderGroups[i].center, max_group_bounds);
        }
    }

}

[System.Serializable]
public class ColliderGroup
{
    public List<MyCollider> colliders = new List<MyCollider>();
    public Vector2 center = new Vector2(); //group中心

    //重新計算中心
    public void UpdateCenter()
    {
        Vector3 _tmp = new Vector3();
        colliders.ForEach(a => _tmp += a.transform.position);
        center = _tmp / (float)colliders.Count;
        Debug.Log("新的center " + center);
    }

    //檢查群組內有物體移動?
    public bool CheckHasMoved()
    {
        return colliders.Exists(a => a.hasMoved == true);
    }



}
