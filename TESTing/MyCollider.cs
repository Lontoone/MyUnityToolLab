//參考:http://davidhsu666.com/archives/gamecollisiondetection/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class MyCollider : MonoBehaviour
{
    public static event Action<MyCollider> eMoved;
    [HideInInspector]
    public bool hasMoved = true;
    public ColliderGroup belongGroup;//屬於哪個碰撞群組?
    public List<MyCollider> current_colliding_objs = new List<MyCollider>();
    public event Action<MyCollider> eOnColliderStay, eOnColliderEnter, eOnColliderLeave;
    public Vector2 border_scale = new Vector2(1, 1);
    public SpriteRenderer spr;
    public List<Vector3> vertices = new List<Vector3>();
    List<Vector3> vertices_originPos = new List<Vector3>(); //vertices原本的位置
    public List<Vector2> normals = new List<Vector2>();
    Mesh colliderMesh;
    Vector2 previous_pos;
    Vector3 previous_rotation;

    MyCollider[] tmp_current_colliders;
    private void Awake()
    {
        spr = gameObject.GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        //spr = gameObject.GetComponent<SpriteRenderer>();
        //Debug.Log(spr.bounds.max);//world
        colliderMesh = SpriteToMesh(spr.sprite);
        //Debug.Log(colliderMesh.bounds.max);//local

        //=== 基本設定 ===
        SetUPVertex();

        //TEST
        tmp_current_colliders = FindObjectsOfType<MyCollider>();

    }
    private void FixedUpdate()
    {
        //=== 檢查移動 ===
        if (previous_pos != (Vector2)transform.position || previous_rotation != transform.eulerAngles)
        {
            SetRotatedPos();
            previous_rotation = transform.eulerAngles;

            if (eMoved != null)
                eMoved(this);

            hasMoved = true;
            previous_pos = transform.position;
        }
        else
            hasMoved = false;

        StayCheck();
    }

    public void SetUPVertex()
    {
        //==設置點連線==
        //用sprite最大、最小點建立box範圍
        //Vector2 sp_max = spr.bounds.max-transform.position;
        //Vector2 sp_min = spr.bounds.min-transform.position;
        Vector2 sp_max = colliderMesh.bounds.max;
        Vector2 sp_min = colliderMesh.bounds.min;

        vertices.Add(sp_min);
        vertices.Add(new Vector2(sp_min.x, sp_max.y));
        vertices.Add(sp_max);
        vertices.Add(new Vector2(sp_max.x, sp_min.y));

        for (int i = 0; i < vertices.Count; i++)
            vertices[i] *= border_scale;

        vertices_originPos = new List<Vector3>(vertices);

        //==找法線==
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 line = vertices[i] - vertices[(i + 1) % vertices.Count];
            normals.Add(new Vector2(line.y, -line.x).normalized);
        }
    }

    public void SetRotatedPos()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = GetRotated_Pos(transform.rotation.z * Mathf.Deg2Rad, vertices_originPos[i], transform.position);
            //vertices[i] = GetRotated_Pos(transform.rotation.z, vertices_originPos[i], transform.position);
        }
        //設定法線
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 line = vertices[i] - vertices[(i + 1) % vertices.Count];
            normals[i] = new Vector2(line.y, -line.x).normalized;
        }
    }

    public void AddCollider(MyCollider newCollider)
    {
        if (newCollider == this)
        {
            return;
        }

        //STAY
        if (current_colliding_objs.Exists(a => a == newCollider))
        {
            if (eOnColliderStay != null)
                eOnColliderStay(newCollider);
        }
        //Enter
        else
        {
            if (eOnColliderEnter != null)
                eOnColliderEnter(newCollider);
            current_colliding_objs.Add(newCollider);
        }
    }
    public void RemoveCollider(MyCollider leavingCollider)
    {
        if (current_colliding_objs.Exists(a => a == leavingCollider) || !belongGroup.colliders.Exists(a => a == leavingCollider))
        {
            current_colliding_objs.Remove(leavingCollider);

            //Leave
            if (eOnColliderLeave != null)
                eOnColliderLeave(leavingCollider);
        }
    }

    Vector3 GetRotated_Pos(float angle, Vector3 currentPos, Vector3 center)
    {

        var new_x = (currentPos.x * Mathf.Cos(angle)) - (currentPos.y * Mathf.Sin(angle));
        var new_y = (currentPos.x * Mathf.Sin(angle)) + (currentPos.y * Mathf.Cos(angle));
        return new Vector2(new_x, new_y);
        //return Quaternion.Euler(0, 0, angle) * (currentPos - center) + center;

    }

    void StayCheck()
    {
        if (current_colliding_objs.Count > 0)
        {
            for (int i = 0; i < current_colliding_objs.Count; i++)
            {
                if (!belongGroup.colliders.Exists(a => a == current_colliding_objs[i]))
                {
                    RemoveCollider(current_colliding_objs[i]);
                }
            }
        }
    }

    public float[] GetMaxMinDot(Vector2 dotNormal, List<Vector3> verties, Transform obj)
    {
        float smallest = 0;
        float biggest = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            float tmp = Vector2.Dot(obj.TransformPoint(vertices[i]), dotNormal);
            if (i == 0)
            {
                smallest = tmp;
                biggest = tmp;
            }
            else
            {
                if (tmp < smallest)
                    smallest = tmp;
                if (tmp > biggest)
                    biggest = tmp;
            }
        }
        return new float[] { smallest, biggest };
    }

    private void OnDrawGizmos()
    {

        for (int i = 0; i < normals.Count; i++)
        {
            Gizmos.DrawLine(transform.position, transform.TransformPoint(normals[i]));
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            Gizmos.DrawWireSphere(transform.TransformPoint(vertices[i]), 0.25f);
        }
    }
    //把sprite轉mesh
    public Mesh SpriteToMesh(Sprite sp)
    {
        //複製一個mesh
        Mesh mesh = new Mesh();
        mesh.SetVertices(Array.ConvertAll(sp.vertices, i => (Vector3)i).ToList());
        mesh.SetUVs(0, sp.uv.ToList());
        mesh.SetTriangles(Array.ConvertAll(sp.triangles, i => (int)i), 0);
        return mesh;
    }

}
