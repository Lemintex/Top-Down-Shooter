using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform muzzle;
    public Projectile projectileType;
    public float timeBetweenShots = 50;
    public float muzzleVelocity = 25;

    float nextShotTime;
    // shoots the gun
    public void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            nextShotTime = Time.time + timeBetweenShots / 1000;
            Projectile newProjectile = Instantiate(projectileType, muzzle.position, muzzle.rotation) as Projectile;
            newProjectile.SetSpeed(muzzleVelocity);
        }
    }
}
