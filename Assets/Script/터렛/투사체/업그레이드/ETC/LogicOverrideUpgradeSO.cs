using UnityEngine;

[CreateAssetMenu(fileName = "NewLogicUpgrade", menuName = "Weapon/Upgrade/Parts/LogicOverride")]
public class LogicOverrideUpgradeSO : WeaponUpgradeSO
{
    public TrackLogicType NewLogicType;

    public override void Apply(WeaponInstance instance)
    {
        instance.LogicType = NewLogicType;
    }
}