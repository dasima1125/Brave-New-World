using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMultiplierUpgrade", menuName = "Weapon/Upgrade/StatMultiplier")]
public class StatMultiplierUpgradeSO : WeaponUpgradeSO
{
    public List<StatModifier> Modifiers;

    public override void Apply(WeaponInstance instance)
    {
        foreach (var mod in Modifiers)
        {
            switch (mod.StatType)
            {
                case StatType.ProjectileSpeed:
                    instance.ProjectileSpeed *= mod.Multiplier;
                    break;
                case StatType.MaxAngularVelocity:
                    instance.MaxAngularVelocity *= mod.Multiplier;
                    break;
                case StatType.LifeTime:
                    instance.LifeTime *= mod.Multiplier;
                    break;
                case StatType.NavigationConstant:
                    instance.NavigationConstant *= mod.Multiplier;
                    break;
            }
        }
    }
}

[Serializable]
public struct StatModifier
{
    public StatType StatType;
    public float Multiplier;
}

public enum StatType
{
    ProjectileSpeed,
    MaxAngularVelocity,
    LifeTime,
    NavigationConstant
}