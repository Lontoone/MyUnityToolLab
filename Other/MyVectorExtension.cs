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
    /// </summary>
    /// <param name="rigidbody"></param>
    /// <param name="originForce">start position</param>
    /// <param name="velocity"></param>
    /// <param name="steps"></param>
    /// <returns></returns>
    public static Vector2[] Plot(Rigidbody2D rigidbody, Vector2 originForce, Vector2 velocity, int steps)
    {
        Vector2[] result = new Vector2[steps];
        float timeStep = Time.fixedDeltaTime / Physics2D.velocityIterations;
        Vector2 gravityAccel = Physics2D.gravity * rigidbody.gravityScale * timeStep * timeStep;

        float drag = 1f - timeStep * rigidbody.drag;
        Vector2 moveStep = velocity * timeStep;

        for (int i = 0; i < steps; i++)
        {
            moveStep += gravityAccel;
            moveStep *= drag;
            originForce += moveStep;
            result[i] = originForce;
        }
        return result;
    }

    
    //3D
    public static float ProjectAngle(Vector3 vec, Vector3 projectAxisX, Vector3 projectAxisY)
    {
        float _mouseProjectedX = Vector3.Dot(vec, projectAxisX);
        float _mouseProjectedY = Vector3.Dot(vec, projectAxisY);

        return Mathf.Atan2(_mouseProjectedY, _mouseProjectedX) * Mathf.Rad2Deg;
    
}
