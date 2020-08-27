using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System;

public class Pointer : MonoBehaviour
{
    public float Length = 5;
    public GameObject Dot;
    public SteamVR_Input_Sources inputSource;
    public SteamVR_Action_Boolean openInput;
    bool isOpen = false;
    private LineRenderer lineRenderer;

    GameObject currentPointing_obj;

    public static event Action<GameObject, Vector3> ePointer_hover;
    public static event Action<GameObject> ePointer_leave;
    public static event Action<GameObject> ePointer_down;
    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.enabled = isOpen;
        Dot.gameObject.SetActive(isOpen);
    }

    private void Update()
    {
        if (openInput.GetStateDown(inputSource))
        {
            //按下按鈕
            if (isOpen)
            {
                RaycastHit down_hit = CreateRaycast(Length);
                if (down_hit.collider != null && ePointer_down != null) { ePointer_down(down_hit.collider.gameObject); }
            }
            isOpen = !isOpen;

            lineRenderer.enabled = isOpen;
            Dot.gameObject.SetActive(isOpen);
        }
        if (!isOpen) { return; }

        //updateLine
        float targetLength = Length;
        RaycastHit hit = CreateRaycast(targetLength);
        Vector3 endPosition = transform.position + (transform.forward * targetLength);

        //check Leave
        if ((hit.collider == null || currentPointing_obj != hit.collider.gameObject) && ePointer_leave != null)
        {
            ePointer_leave(currentPointing_obj);
        }

        if (hit.collider != null)
        {
            endPosition = hit.point;
            if (ePointer_hover != null)
                //hover
                ePointer_hover(hit.collider.gameObject, hit.point);
        }
        currentPointing_obj = hit.collider == null ? null : hit.collider.gameObject;

        Dot.transform.position = endPosition;

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, endPosition);

    }
    private RaycastHit CreateRaycast(float length)
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        Physics.Raycast(ray, out hit, length);
        return hit;
    }
}
