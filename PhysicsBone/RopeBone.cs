using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteAlways]
public class RopeBone : MonoBehaviour
{
    public Vector3 forceGravity = new Vector3(0f, -1.5f,0);
    public int applyTimes = 3;
    public float maxStratch = 0.5f;
    [Range(0f,1.0f)]
    public float friction = 0.9f;
    public float bounce = 1.15f;
    private List<RopePoint> points = new List<RopePoint>();
    private List<Stick> sticks = new List<Stick>();

    [SerializeField] private Transform[] bones;

    private void Start()
    {
        //�[�Jbone
        for (int i = 0; i < bones.Length; i++)
        {
            Vector3 bonePoint = bones[i].transform.position;
            points.Add(new RopePoint(bonePoint, bones[i]));
        }
        //�[�Jstick
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
        // SIMULATION (��C���I��velocity)
        
        for (int i = 0; i < points.Count; i++)
        {
            RopePoint _p = this.points[i];

            //�Ĥ@���I
            if (i == 0) {
                _p.posOld = _p.posNow;
                _p.posNow = _p.boneObject.position;
                points[0] = _p;
                continue;
            }

            //## NewPos = CurrPos + (CurrPos - OldPos)
            Vector3 velocity = (_p.posNow - _p.posOld) *(1-friction);
            _p.posOld = _p.posNow;
            _p.posNow += velocity;
            //�ޤO
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
        /*
        RopePoint firstSegment = this.points[0];
        firstSegment.posNow = bones[0].position;
        this.points[0] = firstSegment;

        int _leng = points.Count;

        RopePoint endSegment = this.points[_leng - 1];
        endSegment.posNow = bones[_leng - 1].position;
        this.points[_leng - 1] = endSegment;

        for (int i = 0; i < _leng - 1; i++)
        {
            RopePoint firstSeg = points[i];
            RopePoint secondSeg = points[i + 1];
            float ropeSegLen = points[i].segLength;

            //�T�w���I�����u�q���סC (�L��=>���Y �A�L�u=>�~�i)
            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude; //�������t�� d = p1 - p2

            //s = (restDistance - |d|)/ |d| =1
            float error = Mathf.Abs(dist - ropeSegLen) ; //�W�X÷�l�T�w���ת��d��(��ҭ� d)   
            Vector3 changeDir = Vector3.zero;

            //�L��
            if (dist > ropeSegLen)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized; //÷�l�~�i�F�h��
            }
            //�L�u
            else if (dist < ropeSegLen)
            {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized; //÷�l���Y�F�h��
            }
            Vector3 changeAmount = changeDir * error ;
            if (i != 0)
            {
                //�N�~�i�Τ��Y���Ȥ��u�b�Y����
                firstSeg.posNow -= changeAmount * 0.5f;
                points[i] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                points[i + 1] = secondSeg;
            }
            else
            {
                secondSeg.posNow += changeAmount;
                points[i + 1] = secondSeg;
            }            
        }*/
    }

    private void DrawRope()
    {
        //�T�w�Ĥ@��
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
        //public float segLength;
        //public Vector3 orginToDir;
        //public Vector3 originLocalPos;
        /*
        public RopeSegment(Vector3 pos, Transform bone, float length, Vector3 fromDir)
        {
            this.posNow = pos;
            this.posOld = pos;
            boneObject = bone;
            segLength = length;
            orginToDir = fromDir;
            originLocalPos = boneObject.localPosition;
        }
        */
        public RopePoint(Vector3 pos, Transform bone)
        {
            this.posNow = pos;
            this.posOld = pos;
            boneObject = bone;
        }
    }
}
