using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashSystem : MonoBehaviour
{
    
    private ParticleSystem particle;
    private List<ParticleCollisionEvent> collisionEvents;

    private void Start()
    {
        particle = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other)
    {
        
        Paintable paintTarget = other.GetComponent<Paintable>();
        if (paintTarget != null)
        {
            //if (RandomChannel) brush.splatChannel = Random.Range(0, 2);

            int numCollisionEvents = particle.GetCollisionEvents(other, collisionEvents);
            for (int i = 0; i < numCollisionEvents; i++)
            {
                //paintTarget.Paint(paintTarget, collisionEvents[i].intersection, collisionEvents[i].normal);
                paintTarget.Paint(collisionEvents[i].intersection);
            }
        }
        
    }

}
