using UnityEngine;

[CreateAssetMenu(fileName = "NewSeekerUpgrade", menuName = "Weapon/Upgrade/Parts/SeekerOverride")]
public class SeekerOverrideUpgradeSO : WeaponUpgradeSO
{
    public SeekerType NewSeekerType;

    public override void Apply(WeaponInstance instance)
    {
        instance.SeekerType = NewSeekerType;
    }
}