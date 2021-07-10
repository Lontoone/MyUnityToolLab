using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float speed = 20;
    public void Update()
    {
        Vector2 move = new Vector2(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"));

        transform.position = (Vector2)transform.position + move * speed * Time.fixedDeltaTime;
    }
}
