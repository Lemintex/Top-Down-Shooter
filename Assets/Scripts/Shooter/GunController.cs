using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform gunHolder;
    public Gun initialGun;
    Gun equippedGun;

    private void Start()
    {
        if(initialGun != null)
        {
           Equip(initialGun);
        }
    }

    // equips gun if none equipped
    public void Equip(Gun gunToEquip)
    {
        if(equippedGun != null)
        {
            Destroy(equippedGun.gameObject);
        }
        equippedGun = Instantiate(gunToEquip, gunHolder.position, gunHolder.rotation) as Gun;
        equippedGun.transform.parent = gunHolder;
    }

    // shoot gun
    public void Shoot()
    {
        equippedGun.Shoot();
    }
}
