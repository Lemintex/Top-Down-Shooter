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

    private void FixedUpdate()
    {
        body.MovePosition(body.position + vel * Time.fixedDeltaTime);
    }
}
