//參考:http://davidhsu666.com/archives/gamecollisiondetection/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
public class MyCollider : MonoBehaviour
{

    public SpriteRenderer spr;
    public Color safe, collideC;
    public List<Vector3> vertices = new List<Vector3>();
    public List<Vector2> normals = new List<Vector2>();
    Mesh colliderMesh;
    Vector2 previous_pos;
    Vector3 previous_rotation;
    Vector3 delta_rotation;

    float _tmp_rotationCounter;

    MyCollider[] tmp_current_colliders;
    private void Start()
    {
        spr = gameObject.GetComponent<SpriteRenderer>();
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
        if (previous_pos != (Vector2)transform.position)
        {
            Debug.Log("移動了");
            CheckCollision(tmp_current_colliders);
            previous_pos = transform.position;
        }

        //=== 檢查旋轉 ===
        if (previous_rotation != transform.eulerAngles)
        {
            delta_rotation = transform.rotation.eulerAngles - previous_rotation;
            if (delta_rotation.z > 180) delta_rotation.z -= 360;
            if (delta_rotation.z < -180) delta_rotation.z += 360;

            _tmp_rotationCounter += delta_rotation.z;

            Debug.Log("旋轉了 " + delta_rotation.z + " 共轉 " + _tmp_rotationCounter);

            SetRotatedPos();
            CheckCollision(FindObjectsOfType<MyCollider>());

            previous_rotation = transform.eulerAngles;
        }
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
            vertices[i] = GetRotated_Pos(delta_rotation.z * Mathf.Deg2Rad * Time.deltaTime, vertices[i], transform.position);
            //vertices[i] = GetRotated_Pos(transform.eulerAngles.z, transform.TransformPoint(vertices[i]), transform.position)-transform.position;

        }
        //設定法線
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 line = vertices[i] - vertices[(i + 1) % vertices.Count];
            normals[i] = new Vector2(line.y, -line.x).normalized;
        }
    }
    public void CheckCollision(MyCollider[] colliders)
    {
        foreach (MyCollider c in colliders)
        {
            if (c == this) { continue; }
            bool isSeperate = false;
            for (int i = 0; i < c.vertices.Count; i++)
            {
                Vector2 current_normal = c.normals[i];
                float[] otherMinMax = c.GetMaxMinDot(current_normal, c.vertices, c.transform);
                float[] thisMinMax = GetMaxMinDot(current_normal, vertices, transform);
                //Debug.Log(" nor " + current_normal + "cMin " + cMin + " max " + cMax + " my min " + myMin + " my max" + myMax);

                Debug.DrawLine(new Vector2(otherMinMax[0], 0), new Vector2(otherMinMax[1], 0), Color.black, 1f);
                Debug.DrawLine(new Vector2(thisMinMax[0], 0), new Vector2(thisMinMax[1], 0), Color.white, 1f);

                if (otherMinMax[0] > thisMinMax[1] || thisMinMax[0] > otherMinMax[1])
                {
                    Debug.Log("<color=green>SAFE!</color>");
                    c.spr.color = safe;
                    spr.color = safe;

                    isSeperate = true;
                    break;
                }
                else
                {
                    Debug.Log("<color=red>碰撞</color>");
                    c.spr.color = collideC;
                    spr.color = collideC;
                }
            }

            if (isSeperate) { continue; }

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 current_normal = normals[i];
                float[] otherMinMax = c.GetMaxMinDot(current_normal, c.vertices, c.transform);
                float[] thisMinMax = GetMaxMinDot(current_normal, vertices, transform);
                Debug.DrawLine(new Vector2(otherMinMax[0], 0), new Vector2(otherMinMax[1], 0), Color.red, 1f);
                Debug.DrawLine(new Vector2(thisMinMax[0], 0), new Vector2(thisMinMax[1], 0), Color.green, 1f);

                if (otherMinMax[0] > thisMinMax[1] || thisMinMax[0] > otherMinMax[1])
                {
                    Debug.Log("<color=green>SAFE!</color>");
                    c.spr.color = safe;
                    spr.color = safe;
                    break;
                }
                else
                {
                    Debug.Log("<color=red>碰撞</color>");
                    c.spr.color = collideC;
                    spr.color = collideC;
                }
            }
        }
    }
    Vector3 GetRotated_Pos(float angle, Vector3 currentPos, Vector3 center)
    {
        /*
        float new_x = (currentPos.x - center.x) * Mathf.Cos(angle) - (currentPos.y - center.y) * Mathf.Sin(angle) + center.x;
        float new_y = (currentPos.y - center.y) * Mathf.Cos(angle) + (currentPos.x - center.x) * Mathf.Sin(angle) + center.y;
        Debug.Log(angle* Mathf.Rad2Deg);*/

        var new_x = (currentPos.x * Mathf.Cos(angle)) - (currentPos.y * Mathf.Sin(angle));
        var new_y = (currentPos.x * Mathf.Sin(angle)) + (currentPos.y * Mathf.Cos(angle));
        return new Vector2(new_x, new_y);



        //return Quaternion.Euler(0, 0, angle) * (currentPos - center) + center;

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
