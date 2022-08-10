using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageableEntity : MonoBehaviour, IDamageable
{
    protected float startingHealth;
    protected float health;
    protected bool alive = true;

    public UnityEvent OnDeath;

    protected virtual void Start()
    {
        health = startingHealth;
    }
    public void Hit(float damage, RaycastHit hitInfo)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    protected void Die()
    {
        alive = false;
        if (OnDeath != null)
        {
            OnDeath.Invoke();
        }
        GameObject.Destroy(gameObject);
    }
}
