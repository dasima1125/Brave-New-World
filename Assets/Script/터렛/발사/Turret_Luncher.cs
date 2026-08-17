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
        // 근데이게맞을까? 추가하는게아니라 걸린건 이니트거는게맞지않나?
        Guided Projectile = Instantiate(Projectileprefeb, pos, Quaternion.Euler(0, 0, fireAngle)).AddComponent<Guided>();
        Projectile.Init(data);
        return Projectile;
    }
}
