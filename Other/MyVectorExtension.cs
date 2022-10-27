using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyVectorExtension
{
    //2D :
    public static Vector2 RotationMatrix2D(Vector2 _originVector, float _radAngle)
    {
        return new Vector2(
            _originVector.x * Mathf.Cos(_radAngle) - _originVector.y * Mathf.Sin(_radAngle),
            _originVector.x * Mathf.Sin(_radAngle) + _originVector.y * Mathf.Cos(_radAngle)
            );
    }
    /// <summary>
    /// Return a vector that clamp the angle between input and normal vector
    /// </summary>
    /// <param name="input">input vector</param>
    /// <param name="normal">the normal vector to base on</param>
    /// <param name="angel">limit angle</param>
    /// <returns></returns>
    public static Vector2 ClampedAngle(Vector2 input, Vector2 normal, float angel)
    {
        float _angleBetween = Vector2.SignedAngle(normal, input);
        if (Mathf.Abs(_angleBetween) <= angel)
        {
            return input;
        }
        else
        {
            //clamp between angle:
            return RotationMatrix2D(normal, angel * Mathf.Deg2Rad * Mathf.Sign(_angleBetween));
        }
    }
    /// <summary>
    /// Predicts each setp's position of adding a force on the rigidbody.
    /// Can use it to draw points of a projectail line.
    /// 預測拋物線路徑
    /// </summary>
    /// <param name="rigidbody"></param>
    /// <param name="originForce">start position</param>
    /// <param name="velocity"></param>
    /// <param name="steps"></param>
    /// <returns></returns>
    public static Vector3[] Plot2D(Rigidbody2D rigidbody, Vector2 originForce, Vector2 velocity, int steps)
    {
        Vector3[] result = new Vector3[steps];
        float timeStep = (Time.fixedDeltaTime + 0.1f) / Physics2D.velocityIterations;
        Vector2 gravityAccel = Physics2D.gravity * rigidbody.gravityScale * timeStep * timeStep;

        float drag = 1f - timeStep * rigidbody.drag;
        Vector2 moveStep = velocity * timeStep;

        for (int i = 0; i < steps; i++)
        {
            moveStep += gravityAccel;
            moveStep *= drag;
            originForce += moveStep;
            result[i] = originForce + (Vector2)rigidbody.transform.position;
        }
        return result;
    }
    public static Vector3[] Plot2D(Rigidbody2D rigidbody, Vector2 originVelocity, Vector2 velocity, float endY, ref Vector3[] result, out int steps)
    {
        //List<Vector3> result = new List<Vector3>();
        float timeStep = (Time.fixedDeltaTime+0.15f) / Physics2D.velocityIterations;
        Vector2 gravityAccel = Physics2D.gravity * rigidbody.gravityScale * timeStep * timeStep;

        float drag = 1f - timeStep * rigidbody.drag;
        Vector2 moveStep = velocity * timeStep;

        Vector2 _currentPos = rigidbody.position;
        steps = 0;
        while(_currentPos.y > endY && steps < result.Length)
        //for (int i = 0; i < steps; i++)
        {            
            moveStep += gravityAccel;
            moveStep *= drag;
            originVelocity += moveStep;
            result[steps] = (originVelocity + (Vector2)rigidbody.transform.position);
            steps++;
        }
        return result;
    }
    //3D
    /// <summary>
    /// 將vec投影在XY軸上取得夾角
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="projectAxisX"></param>
    /// <param name="projectAxisY"></param>
    /// <returns></returns>
    public static float ProjectAngle(Vector3 vec, Vector3 projectAxisX, Vector3 projectAxisY)
    {
        float _mouseProjectedX = Vector3.Dot(vec, projectAxisX);
        float _mouseProjectedY = Vector3.Dot(vec, projectAxisY);

        return Mathf.Atan2(_mouseProjectedY, _mouseProjectedX) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// 取得對稱3D拋物線路徑
    /// </summary>
    /// <param name="v0">初速度</param>
    /// <param name="startPos"></param>
    /// <param name="steps"></param>
    /// <returns></returns>
    public static Vector3[] ProjectilePlots(Vector3 v0, Vector3 startPos, int steps)
    {
        Vector3[] result = new Vector3[steps];
        float _angle = Mathf.Atan2(v0.y, v0.x);
        float _totalTime = (v0.magnitude * 2 * Mathf.Sin(_angle)) / Physics.gravity.magnitude;
        float _step = _totalTime / steps;
        for (int i = 0; i < steps; i++)
        {
            Vector3 pos = CalculatePosInTime(v0, startPos, _step * i);
            result[i] = pos;
        }

        return result;
    }
    /// <summary>
    /// 取得拋物線路徑直到endY高度並回傳總共的seg數量
    /// </summary>
    /// <param name="v0"></param>
    /// <param name="startPos"></param>
    /// <param name="gapDist">each distance between seg. 每段seg距離</param>
    /// <param name="endY"></param>
    /// <param name="segCount"></param>
    /// <returns></returns>
    public static Vector3[] ProjectilePlots(Vector3 v0, Vector3 startPos, float gapDist, float endY, out int segCount)
    {
        segCount = 1;
        //float _angle = Mathf.Atan2(v0.y, v0.x);
        float accX = 0;
        //float currentY = (v0.magnitude * Mathf.Sin(_angle)) * accX - 0.5f * Physics.gravity.magnitude * accX * accX;
        float currentY = CalculatePosInTime(v0, startPos, accX).y;
        List<Vector3> results = new List<Vector3>();
        results.Add(startPos);

        while (currentY > endY && results.Count < 100)
        {
            segCount++;
            accX = segCount * gapDist;
            results.Add(CalculatePosInTime(v0, startPos, accX));
            currentY = results[segCount-1].y;
        }
        return results.ToArray();

    }
    /// <summary>
    /// 計算從start位置以初速度v0幾秒後的位置
    /// </summary>
    /// <param name="vo"></param>
    /// <param name="startPos"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static Vector3 CalculatePosInTime(Vector3 vo, Vector3 startPos, float time)
    {
        Vector3 vxz = vo;
        vxz.y = 0;
        // transform.position=起點
        Vector3 result = startPos + vo * time;
        float sY = (-0.5f * Mathf.Abs(Physics.gravity.y) * time * time) + vo.y * time + startPos.y;

        result.y = sY;
        return result;
    }

    /// <summary>
    /// 輸入目標位置、起點與發射夾角，取得所需所需速度
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <param name="startPos"></param>
    /// <param name="degAngle"></param>
    /// <returns></returns>
    public static Vector3 GetProjectileShootVelocity(Vector3 targetPosition, Vector3 startPos, float degAngle)
    {
        Vector3 p = targetPosition;

        float gravity = Physics.gravity.magnitude;
        // Selected angle in radians
        float angle = degAngle * Mathf.Deg2Rad;

        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPostion = new Vector3(startPos.x, 0, startPos.z);

        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPostion);
        // Distance along the y axis between objects
        float yOffset = startPos.y - p.y;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        // Rotate our velocity to match the direction between the two objects
        //float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion);
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (p.x > startPos.x ? 1 : -1);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        // Fire!
        return finalVelocity;

        // Alternative way:
        // rigid.AddForce(finalVelocity * rigid.mass, ForceMode.Impulse);
    }

}
