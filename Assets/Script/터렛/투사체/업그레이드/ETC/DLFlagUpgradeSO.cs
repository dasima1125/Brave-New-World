using UnityEngine;

[CreateAssetMenu(fileName = "NewDLUpgrade", menuName = "Weapon/Upgrade/Parts/DLFlag")]
public class DLFlagUpgradeSO : WeaponUpgradeSO
{
    public DLMode TargetMode;

    public override void Apply(WeaponInstance instance)
    {
        instance.DLSystem = TargetMode;
    }
}