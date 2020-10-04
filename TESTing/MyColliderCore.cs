using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
//碰撞核心
public class MyColliderCore : MonoBehaviour
{
    /////<summary>來源、接收、法線</summary>
    //public static event Action<MyCollider, MyCollider, Vector2> eOnCollide;
    public List<ColliderGroup> colliderGroups = new List<ColliderGroup>();
    public float group_max_colliders = 4;
    public float group_max_distance = 3;
    /*
    private void FixedUpdate(){
        //canCheck = !canCheck;
        //foreach (ColliderGroup group in colliderGroups) GroupCollision(group);
    }*/
    private void Start()
    {
        MyCollider.eMoved += GroupCollision;
        MyCollider.eMoved += GroupCheck;

        colliderGroups = SetUpAllGroups(group_max_colliders);
        Debug.Log("組數" + colliderGroups.Count);
    }
    private void OnDestroy()
    {
        MyCollider.eMoved -= GroupCollision;
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
                if (CheckIsInsideBoundsCircle(groups[i].center, groups_tmp[j].colliders[0].spr.bounds.extents, groups_tmp[j].colliders[0]))
                {
                    groups[i] = MergeGroup(groups[i], groups_tmp[j].colliders[0]);
                    groups[i].UpdateCenter();
                }
                else if (i == 0)
                {
                    groups.Add(groups_tmp[j]);
                }
            }
        }

        groups.Distinct();

        return groups;
    }

    void MergeGroup(ColliderGroup groupA, ColliderGroup groupB)//B融進A
    {
        for (int i = 0; i < groupB.colliders.Count; i++)
        {
            TransferGroup(groupB, groupA, groupB.colliders[i]);
        }
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

            if (AMinMax[0] > BMinMax[1] || BMinMax[0] > AMinMax[1])
            {
                return false;
            }
        }
        for (int i = 0; i < colliderB.vertices.Count; i++)
        {
            Vector2 current_normal = colliderB.normals[i];
            float[] AMinMax = colliderA.GetMaxMinDot(current_normal, colliderA.vertices, colliderA.transform);
            float[] BMinMax = colliderB.GetMaxMinDot(current_normal, colliderB.vertices, colliderB.transform);
            if (AMinMax[0] > BMinMax[1] || BMinMax[0] > AMinMax[1])
            {
                return false;
            }

        }
        return true;

    }


    //檢查組內碰撞
    public void GroupCollision(MyCollider collider)
    {
        GroupCollision(collider.belongGroup);
    }
    public void GroupCollision(ColliderGroup group)
    {
        //檢查該組有沒有移動過
        if (!group.CheckHasMoved()) { return; }
        if (group.colliders.Count < 2) { group.colliders[0].current_colliding_objs.Clear(); return; }

        int group_colliders_count = group.colliders.Count;

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
    //檢查移動後的物體是否還在該group範圍內
    public void GroupCheck(MyCollider collider)
    {

        //移動距離不大時，跳過
        //if (CheckIsInsideBoundsCircle(collider.belongGroup.center, collider.belongGroup.bounds.extents * 0.5f, collider))return;
        //if (CheckIsInsideBoundsCircle(collider.belongGroup.center, (Vector2)collider.spr.bounds.size * 0.1f, collider)) return;
        //找到有沾到邊的其他群組加入
        for (int i = 0; i < colliderGroups.Count; i++)
        {
            if (collider.belongGroup == colliderGroups[i]) { continue; }
            if (CheckIsInsideBoundsCircle(colliderGroups[i].center, colliderGroups[i].bounds.extents, collider))
            {
                //把目前碰撞中的物體也加進來
                ColliderGroup to = colliderGroups[i];
                //collider.current_colliding_objs.ForEach(a => MergeGroup(a.belongGroup, to));
                MergeGroup(collider.belongGroup, to);
                return;
            }
        }
        //離組的中心太遠
        if ((collider.belongGroup.center - (Vector2)collider.transform.position).magnitude > group_max_distance)
        {
            //創個組給自己
            ColliderGroup self_group = new ColliderGroup();
            //把目前碰撞中的物體也加進來
            collider.current_colliding_objs.ForEach(a => MergeGroup(self_group, a.belongGroup));
            TransferGroup(collider.belongGroup, self_group, collider);
            return;
        }
    }
    public bool CheckIsInsideBoundsCircle(Vector2 center, Vector2 radious, MyCollider c)
    {
        //AABB檢測
        // Rect1
        float minX1 = center.x - radious.x;
        float maxX1 = center.x + radious.x;
        float minY1 = center.y - radious.y;
        float maxY1 = center.y + radious.y;
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
        if (to.colliders.Contains(collider)) { return; }

        //若轉移目標group太多物件了 就踢掉一個叫它自創
        if (to.colliders.Count > group_max_colliders)
        {
            ColliderGroup newto = new ColliderGroup();
            TransferGroup(to, newto, to.colliders[0]);

            return;
        }

        //轉移
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
            //Gizmos.DrawWireSphere(colliderGroups[i].center, max_group_bounds);
            Gizmos.DrawWireCube(colliderGroups[i].center, colliderGroups[i].bounds.size);
        }
    }

    //public MyCollider RayCastHit(Vector2 origin, Vector2 direction, float distance, string tag){}
}

[System.Serializable]
public class ColliderGroup
{
    public List<MyCollider> colliders = new List<MyCollider>();
    public Vector2 center = new Vector2(); //group中心

    public Bounds bounds = new Bounds();

    //重新計算中心
    public void UpdateCenter()
    {
        Vector3 _tmp = new Vector3();
        colliders.ForEach(a => _tmp += a.transform.position);
        center = _tmp / (float)colliders.Count;

        UpdateBounds();
    }

    public void UpdateBounds()
    {
        bounds = new Bounds();
        foreach (MyCollider c in colliders)
        {
            bounds.Encapsulate(c.spr.bounds);
        }

    }

    //檢查群組內有物體移動?
    public bool CheckHasMoved()
    {
        return colliders.Exists(a => a.hasMoved == true);
    }

    public bool CheckContainsAll(List<MyCollider> checks)
    {
        foreach (MyCollider c in checks)
        {
            if (!colliders.Contains(c))
                return false;
        }
        return true;

    }

}
