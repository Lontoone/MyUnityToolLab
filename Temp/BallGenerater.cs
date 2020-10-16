using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//發球器
//物理參考:https://forum.unity.com/threads/how-to-calculate-force-needed-to-jump-towards-target-point.372288/
public class BallGenerater : MonoBehaviour
{

    public bool canShoot = true;
    public float power_multiplier = 1;
    public GameObject ballPrefab;
    public GameObject shoot_target;
    public float shoot_height_angle = 60;

    public LineRenderer lineVisual;
    public int lineSegment;

    //public float shoot_velocity = 50;

    public float shoot_gap_time = 2;

    public bool isRandom = false;
    Coroutine _shoot_coro;

    private void Start()
    {
        if (lineVisual != null)
            lineVisual.positionCount = lineSegment;
    }

    private void FixedUpdate()
    {
        Vector3 dir = -(transform.position - shoot_target.transform.position).normalized;
        //畫拋物線
        //Visualize(dir * shoot_velocity);

        if (_shoot_coro == null && canShoot)
        {
            _shoot_coro = StartCoroutine(Shoot_coro(dir));
        }
    }

    private void OnEnable()
    {
        if (lineVisual != null)
            lineVisual.enabled = true;
    }
    private void OnDisable()
    {
        if (lineVisual != null)
            lineVisual.enabled = false;
    }

    IEnumerator Shoot_coro(Vector3 dir)
    {
        Rigidbody rigidbody = Instantiate(ballPrefab, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
        rigidbody.drag = 0.05f;
        Destroy(rigidbody.gameObject, 10);
        //射擊方向

        //rigidbody.velocity = dir * shoot_velocity;
        rigidbody.velocity = GetShootVelocity();
        if (isRandom)
        {
            rigidbody.velocity += new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)) * 100;
        }

        BallBehavior behavior = rigidbody.GetComponent<BallBehavior>();
        if (behavior != null)
            behavior.ballScore.startPos = transform.position;

        ScoringManager.currentBall++;
        yield return new WaitForSeconds(shoot_gap_time);
        _shoot_coro = null;
    }



    //畫出拋物線
    void Visualize(Vector3 vo)
    {
        for (int i = 0; i < lineSegment; i++)
        {
            Vector3 pos = CalculatePosInTime(vo, i / (float)lineSegment);
            lineVisual.SetPosition(i, pos);
        }
    }
    Vector3 CalculatePosInTime(Vector3 vo, float time)
    {
        Vector3 vxz = vo;
        vxz.y = 0;
        // transform.position=起點
        Vector3 result = transform.position + vo * time;
        float sY = (-0.5f * Mathf.Abs(Physics.gravity.y) * time * time) + vo.y * time + transform.position.y;

        result.y = sY;
        return result;
    }
    //計算拋物線
    Vector3 GetShootVelocity()
    {
        Vector3 p = shoot_target.transform.position;

        float gravity = Physics.gravity.magnitude;
        // Selected angle in radians
        float angle = shoot_height_angle * Mathf.Deg2Rad;

        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPostion = new Vector3(transform.position.x, 0, transform.position.z);

        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPostion);
        // Distance along the y axis between objects
        float yOffset = transform.position.y - p.y;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        // Rotate our velocity to match the direction between the two objects
        //float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion);
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (p.x > transform.position.x ? 1 : -1);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        // Fire!
        //rigid.velocity = finalVelocity;
        //Debug.Log(finalVelocity);
        return finalVelocity;

        // Alternative way:
        // rigid.AddForce(finalVelocity * rigid.mass, ForceMode.Impulse);
    }
}
