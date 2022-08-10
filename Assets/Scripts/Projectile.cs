using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask layer;
    public float radius;
    private float speed = 10;
    public float damage = 10;

    float lifetime = 3;

    void Start()
    {
        Destroy(gameObject, lifetime);// destroy object after a few seconds    
    }
    void Update()
    {
        float moveAmount = speed * Time.deltaTime;
        CheckCollisions(moveAmount);
        transform.Translate(Vector3.forward * moveAmount);
    }

    // called by gun to set the speed of the fired projectile
    public void setSpeed(float s)
    {
        speed = s;
    }
    // TODO: try a spherecast instead
    void CheckCollisions(float moveAmount)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, moveAmount, layer, QueryTriggerInteraction.Collide))//, layer))
        {
            OnObjectHit(hitInfo);
        }
    }

    // called when the projectile hits an entity
    void OnObjectHit(RaycastHit hitInfo)
    {
        IDamageable hitObject = hitInfo.collider.GetComponent<IDamageable>();
        if (hitObject != null)
        {
            hitObject.Hit(damage, hitInfo);
        }
        GameObject.Destroy(gameObject);
    }
}
