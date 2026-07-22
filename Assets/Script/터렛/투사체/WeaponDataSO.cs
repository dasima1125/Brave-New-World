using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapon/WeaponData")]
public class WeaponDataSO : ScriptableObject
{
    [Header("미사일 식별")]
    public MissileModel MissileModel;
    public Sprite MissileSprite;
    public int Price; 

    [Header("미사일 파라미터")]    
    public SeekerType SeekerType;
    public TrackLogicType LogicType;
    public DLMode DLSystem;
    public float ProjectileSpeed;
    public float MaxAngularVelocity;
    public float LifeTime;
    public float NavigationConstant;

    [Header("가능한 튜닝옵션")]
    public List<UpgradeSlots> AvailableUpgrades;

    private void OnValidate()
    {
        var usedSlots = new HashSet<UpgradeSlot>();
        foreach (var entry in AvailableUpgrades)
        {
            if (usedSlots.Contains(entry.Slot))
            {
                Debug.LogWarning($"[{name}] 슬롯 중복 감지: {entry.Slot} → 자동 변경됨");
                foreach (UpgradeSlot slot in Enum.GetValues(typeof(UpgradeSlot)))
                {
                    if (!usedSlots.Contains(slot))
                    {
                        entry.Slot = slot;
                        break;
                    }
                }
            }
            usedSlots.Add(entry.Slot);
        }
    }
}
[Serializable]
public class UpgradeSlots
{
    public UpgradeSlot Slot;
    public WeaponUpgradeSO DefaultUpgrade;
    public List<WeaponUpgradeSO> Upgrades;
}
