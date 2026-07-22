using UnityEngine;

public class Turret_Luncher : MonoBehaviour
{
    Turret_Core _core;
    [SerializeField] GameObject Projectileprefeb;
    public void Init(Turret_Core core)
    {
        _core = core;
    }
    public Guided CreateProjectile(WeaponData data ,Vector3 pos , float fireAngle)
    {
        Guided Projectile = Instantiate(Projectileprefeb, pos, Quaternion.Euler(0, 0, fireAngle)).AddComponent<Guided>();
        Projectile.Init(data);
        return Projectile;
    }
}
