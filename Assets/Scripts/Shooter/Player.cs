using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(PlayerController))]
[RequireComponent (typeof(GunController))]
public class Player : DamageableEntity
{
    public float speed = 5f;

    Camera view;
    PlayerController playerController;
    GunController gunController;
    protected override void Start()
    {
        startingHealth = 5;// set before base Start() to correctly initialise health

        base.Start();
        playerController = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        view = Camera.main;
    }

    void Update()
    {
        // movement
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 vec = input.normalized * speed;
        playerController.Move(vec);

        // look direction
        Ray ray = view.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        float distance;
        if (ground.Raycast(ray, out distance))
        {
            Vector3 point = ray.GetPoint(distance);
            playerController.LookAt(point);
        }

        // shootng
        if(Input.GetMouseButton(0))
        {
            gunController.Shoot();
        }
    }
}
