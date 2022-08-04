using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PlayerController))]
public class Player : MonoBehaviour
{
    public float speed = 5f;

    Camera view;
    PlayerController playerController;
    GunController gunController;
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        view = Camera.main;
    }

    void Update()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        playerController.Move(input);

        Ray ray = view.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        float distance;
        if (ground.Raycast(ray, out distance))
        {
            Vector3 point = ray.GetPoint(distance);
            Debug.DrawLine(ray.origin, point, Color.red);
            playerController.LookAt(point);
        }

        if(Input.GetMouseButton(0))
        {
            gunController.Shoot();
        }
    }
}
