using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteAlways]
public class RopeBone : MonoBehaviour
{
    public Vector3 forceGravity = new Vector3(0f, -1f,0);
    public int applyTimes = 3;
    
    public float mass = 1;
    public float stiffness = 0.1f;
    [Range(0f,1.0f)]
    public float damping = 0.9f;
    //public float friction = 0.9f;
    private List<RopePoint> points = new List<RopePoint>();
    private List<Stick> sticks = new List<Stick>();

    [SerializeField] private Transform[] bones;

    private void Start()
    {
        //加入bone
        for (int i = 0; i < bones.Length; i++)
        {
            Vector3 bonePoint = bones[i].transform.position;
            RopePoint rp = new RopePoint(bonePoint, bones[i]);
            rp.originLocalPos = transform.InverseTransformPoint(bonePoint);
            points.Add(rp);
        }
        //加入stick
        for (int i = 0; i<points.Count-1;i++) {
            sticks.Add(new Stick( points[i], points[i + 1]));
        }

    }
    private void Update()
    {
        this.DrawRope();
    }
    private void FixedUpdate()
    {
        this.Simulate();
    }

    private void Simulate()
    {
        // SIMULATION (算每個點的velocity)
        
        for (int i = 0; i < points.Count; i++)
        {
            RopePoint _p = this.points[i];

            //第一個點
            if (i == 0) {
                _p.posOld = _p.posNow;
                _p.posNow = _p.boneObject.position;
                points[0] = _p;
                continue;
            }

            //## NewPos = CurrPos + (CurrPos - OldPos)
            Vector3 force = (_p.posNow - transform.TransformPoint(_p.originLocalPos)) * stiffness ; //與原本位置的補正
            Vector3 velocity = (_p.posNow - _p.posOld) *(damping);
            mass = Mathf.Max(0.01f,mass);
            velocity = (velocity - force/mass);
            _p.posOld = _p.posNow;
            _p.posNow += velocity;
            //引力
            _p.posNow += forceGravity * Time.fixedDeltaTime;
            points[i] = _p;
        }

        //CONSTRAINTS
        for (int i = 0; i < applyTimes; i++)
        {
            this.ApplyConstraint();
        }
    }

    private void ApplyConstraint()
    {
        for (int i =0; i<sticks.Count;i++) {
            Vector3 moveDir = sticks[i].p1.posNow - sticks[i].p0.posNow;
            float dis = moveDir.magnitude;
            float diff = sticks[i].length - dis;
            float percent = diff / dis / 2;

            sticks[i].p0.posNow -= percent * moveDir;
            sticks[i].p1.posNow += percent * moveDir;
        }        
    }

    private void DrawRope()
    {
        //固定第一個
        for (int i = 1; i < points.Count; i++)
        {
            points[i].boneObject.position = points[i].posNow;
            /*
            RopeSegment _rope = ropeSegments[i];
            _rope.posNow = Vector3.Lerp(_rope.posNow, _rope.originLocalPos, Time.fixedDeltaTime);
            _rope.boneObject.position = _rope.posNow;
            */
        }

    }

    public class Stick {
        public RopePoint p0, p1;
        public float length;

        public Stick(RopePoint _p0, RopePoint _p1 ) {
            p0 = _p0;
            p1 = _p1;

            length = (_p0.boneObject.position - _p1.boneObject.position).magnitude;
        }

    }
    public class RopePoint
    {
        public Vector3 posNow;
        public Vector3 posOld;
        public Transform boneObject;
        public Vector3 originLocalPos;
        
        public RopePoint(Vector3 pos, Transform bone)
        {
            this.posNow = pos;
            this.posOld = pos;
            boneObject = bone;
        }
    }
}
