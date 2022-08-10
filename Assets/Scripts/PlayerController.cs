using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Vector3 vel;
    Rigidbody body;

    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    public void Move(Vector3 input)
    {
        vel = input;
    }

    // points the player at the cursor
    public void LookAt(Vector3 point)   
    {
        Vector3 lookPoint = new Vector3(point.x, transform.position.y, point.z);
        transform.LookAt(lookPoint);
    }

    private void FixedUpdate()
    {
        body.MovePosition(body.position + vel * Time.fixedDeltaTime);
    }
}
