using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl3D : MonoBehaviour
{

    public Vector2 sensitive;

    public GameObject followTarget;
    public GameObject cameraCenter;

    public float speed = 5;
    public float cameraHeight = 1;

    private float _defaultCameraDist;
    public float currentZoom = 1;


    public LayerMask groundLayer;
    private Camera _camera;
    private Vector3 _previousMousePos;


    private void Start()
    {
        _camera = Camera.main;
        _defaultCameraDist = Vector3.Distance(cameraCenter.transform.position, _camera.transform.position);

        _previousMousePos = _camera.ScreenToViewportPoint(Input.mousePosition);
    }

    private void FixedUpdate()
    {
        //轉(x,y)
        Vector3 _mousePos = _camera.ScreenToViewportPoint(Input.mousePosition);
        //_mousePos.z = -10;
        Vector3 _deltaCamerPosMove = (_mousePos - _previousMousePos) * sensitive;
        _previousMousePos = _mousePos;

        //Debug.Log(-_deltaCamerPosMove.y + " " + _camera.transform.localEulerAngles.x);
        //float _xClampedAngle =  Mathf.Clamp(-_deltaCamerPosMove.y + _camera.transform.localEulerAngles.x, -80, 360);

        float _xClampedAngle = -_deltaCamerPosMove.y + cameraCenter.transform.localEulerAngles.x;
        float _yClampedAngle = _deltaCamerPosMove.x + cameraCenter.transform.localEulerAngles.y;//Mathf.Clamp(_deltaCamerPosMove.x + _camera.transform.localEulerAngles.y, -60, 60);
        /*

        _camera.transform.localEulerAngles = new Vector3(
            _xClampedAngle,
            _yClampedAngle,
            0);
            */
        Quaternion _rotation = Quaternion.Euler(
            _xClampedAngle,
            _yClampedAngle,
            0);

        cameraCenter.transform.rotation = _rotation;

        FollowTarget();
        CameraCollisionRestrict();
        //ApplyZoom();
    }

    private void FollowTarget()
    {
        Vector3 _move = Vector3.Lerp(
            cameraCenter.transform.position,
            followTarget.transform.position,
            Time.fixedDeltaTime * speed);
        cameraCenter.transform.position = _move;
    }

    private void CameraCollisionRestrict()
    {
        RaycastHit _hit;
        Debug.DrawLine(_camera.transform.position, cameraCenter.transform.position, Color.red);
        if (Physics.Linecast(_camera.transform.position, cameraCenter.transform.position, out _hit, groundLayer))
        {
            Debug.Log("hit ground " + _hit.transform.name);
            Vector3 _strictPos = _hit.transform.position;
            //_camera.transform.position = _strictPos;

            //calculate zoom:
            currentZoom = Mathf.Clamp01(
                Vector3.Distance(_hit.transform.position, cameraCenter.transform.position) / _defaultCameraDist);

        }
        else
        {
            currentZoom = Mathf.Lerp(currentZoom, 1, Time.fixedDeltaTime * 20);
        }
    }

    private void ApplyZoom()
    {
        //get dir:
        Vector3 _dir = (_camera.transform.position - cameraCenter.transform.position).normalized;
        Vector3 _maxDist = _dir * _defaultCameraDist;
        _camera.transform.position = Vector3.Lerp(cameraCenter.transform.position, _maxDist, currentZoom);
    }
}
