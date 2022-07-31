using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PlayerController))]
public class Player : MonoBehaviour
{
    public float speed = 5f;
    PlayerController controller;
    void Start()
    {
        controller = GetComponent<PlayerController>();
    }

    void Update()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        controller.Move(input);
    }
}
