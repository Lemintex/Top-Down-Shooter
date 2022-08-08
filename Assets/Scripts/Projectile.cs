using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask layer;
    public float radius;
    float speed = 10;
    public float damage = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float moveAmount = speed * Time.deltaTime;
        CheckCollisions(moveAmount);
        transform.Translate(Vector3.forward * moveAmount);
    }
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
