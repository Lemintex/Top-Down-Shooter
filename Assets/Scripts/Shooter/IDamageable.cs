using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void Hit(float damage, RaycastHit hitInfo);
    void Damage(float damage);
}
