using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableEntity : MonoBehaviour, IDamageable
{
    protected float startingHealth;
    protected float health;
    protected bool alive;

    protected virtual void Start()
    {
        health = startingHealth;
    }
    public void Hit(float damage, RaycastHit hitInfo)
    {
        Debug.Log(health);
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    protected void Die()
    {
        alive = false;
        GameObject.Destroy(gameObject);
    }
}
