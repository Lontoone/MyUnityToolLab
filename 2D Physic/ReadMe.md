The code is still in development stage. (sorry,i misspelled "physic" which should be "physics")
腳本還在開發階段

It is a pseudo physic script that only tries to make sense, and not phyically accurate.


demo:

![image](https://i.imgur.com/augrQM0.gif)
The balls are applied with my basic movement AI. so they will move randomly, and jump when they hit wall.

How To Use [My2DPhysic.cs]:

  Before start, make sure you have 2 layers, which indicates what is wall and what is ground.
  
  This script will simulte gravity,but not colliding event, if you want to collide with others, apply [My2DCollider.cs] as well.
  
  Parameter explain:
  
     public bool isEntity: true if this object is collideable. same concept as Unity collider's [isTrigger] function.
     public Vector2 force : value that push the gameobject, use Addforce(vector2) to change value is recommended.
     public bool freezeRotation_z: The same function as Unity's rigidboyd ["freeze rotation z"]
     public float angular_acceleration : if freezeRotation_z is unchecked, it controls the speed of z-rotation when object is on a slope.
     public float friction_self: the friction when the object is on a slope.
     public float forceDrag : If the value is smaller, it will be more bouncy.
    
  use example:
    if player want to move, write sth like this:
    * rigid = gameObject.GetComponent<My2Dphysic>();
    
     private void Update(){
        rigid.velocity.x = Input.GetAxis("Horizontal") * your_speed;
     }
    
   if player want to jump, write sth like this:
   
     if (Input.GetKeyDown(KeyCode.Space) && rigid.isGrounded)
       {
            rigid.AddForce(Vector2.up * your_jumpForce);
       }

How To Use [My2DCollider.cs]:

  This script requires unity Collider2D component.
  
  Because unity onCollision__ event will only be triggered when rigidbody2D is attached, which is replaced by My2DPhysic.cs is this case. So this script is meant to take charge of the event triggering.
  
  It will work fine in defult with My2DPhysic.
  The parameter [public LayerMask entity_layer] meant what layer's object can trigger the event.
  
  If the mass is greater, the object will give more push-force to other objects when them collide.
