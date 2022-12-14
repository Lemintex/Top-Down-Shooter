using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask layer;
    public float radius;
    private float speed = 10;
    public float damage = 10;
    float fastCompensation = 0.1f;

    float lifetime = 2;

    void Start()
    {
        Destroy(gameObject, lifetime);// destroy object after a few seconds
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, 0.1f, layer);
        if (initialCollisions.Length > 0)
        {
            OnObjectHit(initialCollisions[0]);
        }
    }
    void Update()
    {

        float moveAmount = speed * Time.deltaTime;
        CheckCollisions(moveAmount + fastCompensation);// fastCompensation is added to ensure bullet hits if the enemy barely dodges
        transform.Translate(Vector3.forward * moveAmount);
    }

    // called by gun to set the speed of the fired projectile
    public void SetSpeed    (float s)
    {
        speed = s;
    }

    // TODO: try a spherecast instead
    void CheckCollisions(float moveAmount)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, moveAmount, layer, QueryTriggerInteraction.Collide))
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

    // called when a projectile spawns inside an entity
    void OnObjectHit(Collider collider)
    {
        if (collider != null)
        {
            IDamageable hitObject = collider.GetComponent<IDamageable>();
            hitObject.Damage(damage);
        }
        GameObject.Destroy(gameObject);
    }
}
