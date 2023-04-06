using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
[ExecuteInEditMode]
public class GPUInstancer : MonoBehaviour
{
    public Vector3 offset;
    public Vector3 maxPostion;
    public Vector3 maxScale = Vector3.one;
    public Vector3 minScale;
    public Vector3 minRotation;
    public Vector3 maxRotation;

    public int instanceCount = 100000;
    public Mesh instanceMesh;
    public Material instanceMaterial;
    public int subMeshIndex = 0;
    private int currentCount = 0;

    List<GPUPoint> points= new List<GPUPoint>();
    int[] drawArgs;
    private ComputeBuffer cbDrawArgs;
    public ComputeBuffer pointsBuffer;

    public struct GPUPoint
    {
        public Vector3 position;
        public Vector3 rotation; //TODO: Change to Normal
        public Vector3 scale;
        public Vector4 color;
        //public float depth;
    }

    private void Start()
    {
        //SetUp();

    }
    private void OnValidate()
    {
        //SetUp();
    }
    public void SetUpPoints(Vector3 position)
    {
        GPUPoint point = new GPUPoint();
        Vector3 relativePos = position - transform.position;
        point.position = relativePos;
        point.rotation = Quaternion.Euler(new Vector3(Random.Range(minRotation.x, maxRotation.x), Random.Range(minRotation.y, maxRotation.y), Random.Range(minRotation.z, maxRotation.z))).eulerAngles;
        point.scale = new Vector3(Random.Range(minScale.x, maxScale.x), Random.Range(minScale.y, maxScale.y), Random.Range(minScale.z, maxScale.z));
        Color rndColor = Random.ColorHSV();
        point.color = rndColor;
        currentCount++;
        if (points.Count < instanceCount)
        {
            points.Add(point);
        }
        else
        {
            points[currentCount % instanceCount] = point;
        }
        //Debug.Log(points.Count+" "+ position);
        SetUp();
    }
    private void SetUp()
    {
        //points = new GPUPoint[instanceCount].ToList();
        drawArgs = new int[]
            {
                (int)instanceMesh.GetIndexCount(0),
                //instanceCount,
                points.Count,
                (int)instanceMesh.GetIndexStart(0),
                (int)instanceMesh.GetBaseVertex(0),
                0
        };
        if (pointsBuffer == null)
        {
            int strip = Marshal.SizeOf(typeof(GPUPoint));
            pointsBuffer = new ComputeBuffer(instanceCount, strip);
        }
        //設定Mesh資料

        if (cbDrawArgs == null)
        {

            cbDrawArgs = new ComputeBuffer(1, drawArgs.Length * 4, ComputeBufferType.IndirectArguments); //each int is 4 bytes
        }
        
        pointsBuffer.SetData(points);
        cbDrawArgs.SetData(drawArgs);
    }

    private void Update()
    {

        if (cbDrawArgs == null)
        {
            return;
        }
        instanceMaterial.SetBuffer("_PointsBuffer", pointsBuffer);        
        instanceMaterial.SetVector("transform_center", new Vector4(transform.position.x , transform.position.y , transform.position.z   ,0));

        //Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(transform.position , maxPostion), cbDrawArgs);        
        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(Vector3.zero, maxPostion), cbDrawArgs);
        //Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(transform.localPosition +maxPostion*0.5f, maxPostion), cbDrawArgs);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(transform.position + offset, maxPostion);

        foreach (GPUPoint p in points)
        {
            //Gizmos.DrawWireSphere(p.position , 0.5f);

        }
    }
}