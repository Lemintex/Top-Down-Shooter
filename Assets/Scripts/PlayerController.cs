using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody ))]
public class PlayerController : MonoBehaviour
{
    Vector3 vel;
    Rigidbody body;
    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move(Vector3 input)
    {
        vel = input.normalized;
    }

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
