using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
[RequireComponent(typeof(HitableObj))]
public class BreakToPiece : MonoBehaviour
{
    SpriteRenderer spr;
    public Vector2 minVertexRandom = new Vector2(1, 1);
    public int subDivide = 1;
    [Tooltip("用到動畫spriteSheet才要打")]
    public string img_resource_path = "Img/";
    public event Action<GameObject> eCreateNewPiece;
    public float gapTime = 0.01f;
    public bool doRectBox = true;//產生方形而不是三角形的碎片
    HitableObj hitable;
    private void Start()
    {
        hitable = gameObject.GetComponent<HitableObj>();
        hitable.Die_event += DoBreak;
    }
    private void OnDestroy()
    {
        hitable.Die_event -= DoBreak;
    }
    private void DoBreak()
    {
        StartCoroutine(BreakSlowly_coro());
    }

    IEnumerator BreakSlowly_coro()
    {
        spr = gameObject.GetComponent<SpriteRenderer>();

        int n = subDivide + 2; //加頭尾

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        //int[] tris = new int[(n - 1) * (n - 1) * 6];
        List<int> tris = new List<int>();
        Mesh subDividedMesh = new Mesh();

        //分布點位置
        float uv_ratio = 1f / (float)(n - 1);
        Vector2 vertices_ratio = spr.sprite.bounds.size / (n - 1); //比例尺
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                //隨機(0~1)
                Vector2 ran = new Vector2(UnityEngine.Random.Range(minVertexRandom.x, 1f), UnityEngine.Random.Range(minVertexRandom.y, 1f));
                Vector2 v = new Vector2(i * vertices_ratio.x * ran.x, j * vertices_ratio.y * ran.y);
                vertices.Add(v);

                Vector2 uv = new Vector2(i, j) * uv_ratio;
                uvs.Add(uv);
            }
        }

        //三角形
        for (int xi = 0; xi < n - 1; xi++)
        {
            for (int yi = 0; yi < n - 1; yi++)
            {
                //正方形左上角= nXi+Yi , nXi+Yi+1 , n(Xi+1)+Yi+1
                int[] left_tri = new int[] {
                    n * xi + yi,
                    n*xi+yi+1,
                    n*(xi+1)+yi+1 };

                //正方形右下角= nXi+Yi , n(Xi+1)+Yi+1 , n(Xi+1)+Yi
                int[] right_tri = new int[] {
                    n * xi + yi,
                    n*(xi+1)+yi+1,
                    n*(xi+1)+yi };

                tris.AddRange(left_tri);
                tris.AddRange(right_tri);
            }
        }

        subDividedMesh.SetVertices(vertices);
        subDividedMesh.SetUVs(0, uvs);
        subDividedMesh.SetTriangles(tris, 0);

        //拆成更小的
        //四角形的
        if (doRectBox)
        {
            for (int i = 0; i < subDividedMesh.triangles.Length - 5; i += 6)
            {
                Mesh newPiece = SeparateMesh_rect(subDividedMesh, i);

                //位置處理
                GameObject newSubMesh_obj = new GameObject();
                newSubMesh_obj.transform.position = transform.position - subDividedMesh.bounds.center * 0.5f;
                newSubMesh_obj.transform.localScale = transform.localScale;

                newSubMesh_obj.AddComponent<MeshFilter>().mesh = newPiece;

                //貼圖處理 (動畫sheet需要拆解)
                //Material material = new Material(shader);
                Material material = spr.material;
                if (img_resource_path != "")
                    material.mainTexture = ConvertSpriteToTexture(GetSpriteFromSheet(spr.sprite.texture, spr.sprite.name));
                else
                    material.mainTexture = spr.sprite.texture;

                newSubMesh_obj.AddComponent<MeshRenderer>().material = material;

                //給後續處理
                if (eCreateNewPiece != null)
                    eCreateNewPiece(newSubMesh_obj);

                yield return new WaitForSeconds(gapTime);
                //刪掉原本的
                //Destroy(gameObject);
            }
        }
        //三角形的
        else
        {
            for (int i = 0; i < subDividedMesh.triangles.Length - 2; i += 3)
            {
                Mesh newPiece = SeparateMesh(subDividedMesh, i);

                //位置處理
                GameObject newSubMesh_obj = new GameObject();
                newSubMesh_obj.transform.position = transform.position - subDividedMesh.bounds.center * 0.5f;
                newSubMesh_obj.transform.localScale = transform.localScale;

                newSubMesh_obj.AddComponent<MeshFilter>().mesh = newPiece;

                //貼圖處理 (動畫sheet需要拆解)
                //Material material = new Material(shader);
                Material material = spr.material;
                if (img_resource_path != "")
                    material.mainTexture = ConvertSpriteToTexture(GetSpriteFromSheet(spr.sprite.texture, spr.sprite.name));
                else
                    material.mainTexture = spr.sprite.texture;

                newSubMesh_obj.AddComponent<MeshRenderer>().material = material;

                //給後續處理
                if (eCreateNewPiece != null)
                    eCreateNewPiece(newSubMesh_obj);

                yield return new WaitForSeconds(gapTime);
                //刪掉原本的
                //Destroy(gameObject);
            }
        }
        Destroy(gameObject);
    }

    public Mesh SeparateMesh(Mesh _oldMesh, int startIndex)
    {
        Mesh pieceMesh = new Mesh();
        Vector3[] vertices = new Vector3[3];
        int[] old_triangle = new int[3]{_oldMesh.triangles[startIndex],
                                    _oldMesh.triangles[startIndex+1],
                                    _oldMesh.triangles[startIndex+2]};
        vertices[0] = _oldMesh.vertices[old_triangle[0]];
        vertices[1] = _oldMesh.vertices[old_triangle[1]];
        vertices[2] = _oldMesh.vertices[old_triangle[2]];
        pieceMesh.vertices = vertices;
        pieceMesh.triangles = new int[3] { 0, 1, 2 };
        Vector2[] uv = new Vector2[3];
        uv[0] = _oldMesh.uv[old_triangle[0]];
        uv[1] = _oldMesh.uv[old_triangle[1]];
        uv[2] = _oldMesh.uv[old_triangle[2]];
        pieceMesh.uv = uv;
        return pieceMesh;
    }
    public Mesh SeparateMesh_rect(Mesh _oldMesh, int startIndex)
    {
        Mesh pieceMesh = new Mesh();
        Vector3[] vertices = new Vector3[4];
        int[] old_triangle = new int[4]{_oldMesh.triangles[startIndex],
                                    _oldMesh.triangles[startIndex+1],
                                    _oldMesh.triangles[startIndex+2],
                                    _oldMesh.triangles[startIndex+5]};
        vertices[0] = _oldMesh.vertices[old_triangle[0]];
        vertices[1] = _oldMesh.vertices[old_triangle[1]];
        vertices[2] = _oldMesh.vertices[old_triangle[2]];
        vertices[3] = _oldMesh.vertices[old_triangle[3]];
        pieceMesh.vertices = vertices;
        pieceMesh.triangles = new int[6] { 0, 3, 2, 0, 2, 1 };
        Vector2[] uv = new Vector2[4];
        uv[0] = _oldMesh.uv[old_triangle[0]];
        uv[1] = _oldMesh.uv[old_triangle[1]];
        uv[2] = _oldMesh.uv[old_triangle[2]];
        uv[3] = _oldMesh.uv[old_triangle[3]];
        pieceMesh.uv = uv;
        return pieceMesh;
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

    public Sprite GetSpriteFromSheet(Texture2D texture, string spriteName)
    {
        Debug.Log(texture.name + " " + spriteName);
        Sprite[] sprites = Resources.LoadAll<Sprite>(img_resource_path + texture.name);
        Sprite resulte = sprites.Single(s => s.name == spriteName);
        Debug.Log(resulte.name);
        return resulte;
    }
    Texture2D ConvertSpriteToTexture(Sprite sprite)
    {
        try
        {
            if (sprite.rect.width != sprite.texture.width)
            {
                Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] colors = newText.GetPixels();
                Color[] newColors = sprite.texture.GetPixels((int)System.Math.Ceiling(sprite.textureRect.x),
                                                             (int)System.Math.Ceiling(sprite.textureRect.y),
                                                             (int)System.Math.Ceiling(sprite.textureRect.width),
                                                             (int)System.Math.Ceiling(sprite.textureRect.height));
                Debug.Log(colors.Length + "_" + newColors.Length);
                newText.SetPixels(newColors);
                newText.Apply();
                return newText;
            }
            else
                return sprite.texture;
        }
        catch
        {
            return sprite.texture;
        }
    }

}
