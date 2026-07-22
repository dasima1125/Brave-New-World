using System.Collections.Generic;

public class WeaponInstance
{
    public MissileModel MissileModel;
    public SeekerType SeekerType;
    public TrackLogicType LogicType;
    public DLMode DLSystem;
    public float ProjectileSpeed;
    public float MaxAngularVelocity;
    public float LifeTime;
    public float NavigationConstant;

    public WeaponInstance(WeaponDataSO baseData, List<WeaponUpgradeSO> appliedUpgrades)
    {
        // 원본값 사본
        MissileModel       = baseData.MissileModel;
        SeekerType         = baseData.SeekerType;
        LogicType          = baseData.LogicType;
        DLSystem           = baseData.DLSystem;
        ProjectileSpeed    = baseData.ProjectileSpeed;
        MaxAngularVelocity = baseData.MaxAngularVelocity;
        LifeTime           = baseData.LifeTime;
        NavigationConstant = baseData.NavigationConstant;

        // 업그레이드 순서대로 적용
        foreach (var upgrade in appliedUpgrades)
        {
            upgrade.Apply(this);
        }
    }
}