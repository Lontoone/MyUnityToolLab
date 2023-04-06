using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paintable : MonoBehaviour
{
    public GPUInstancer mGPUInstancer;
    private ParticleSystem particle;
    private List<ParticleCollisionEvent> collisionEvents;

    private void Start()
    {
        mGPUInstancer = gameObject.GetComponent<GPUInstancer>();
        
    }

    public void Paint(Vector3 position)
    {
        if (mGPUInstancer == null) {
            return;
        }
        mGPUInstancer.SetUpPoints(position);
    }
}