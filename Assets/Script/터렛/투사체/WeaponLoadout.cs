
using System.Collections.Generic;

public class WeaponLoadout
{
    public WeaponDataSO BaseData;
    public Dictionary<UpgradeSlot, List<WeaponUpgradeSO>> AllUpgradeParts = new();      //등록된 파츠 슬롯과 파츠들
    public Dictionary<UpgradeSlot, List<WeaponUpgradeSO>> UnLockedUpgradeParts = new(); // 사용가능한 파츠들
    private Dictionary<UpgradeSlot, WeaponUpgradeSO> _appliedUpgrades = new();          // 장착한 파츠

    public WeaponLoadout(WeaponDataSO baseData)
    {
        BaseData = baseData;

        foreach (var entry in baseData.AvailableUpgrades)
        {
            // 업그레이드 등록
            AllUpgradeParts[entry.Slot] = entry.Upgrades;

            // 기본형은 처음부터 해금
            if (entry.DefaultUpgrade != null)
            {
                UnLockedUpgradeParts[entry.Slot] = new List<WeaponUpgradeSO> { entry.DefaultUpgrade };
                _appliedUpgrades[entry.Slot] = entry.DefaultUpgrade;
            }
        }
    }
    #region 슬롯 온오프 관리
    public bool Unlock(WeaponUpgradeSO upgrade)
    {

        if (!AllUpgradeParts.TryGetValue(upgrade.Slot, out var all)) return false;
        if (!all.Contains(upgrade)) return false;
        if (!UnLockedUpgradeParts.ContainsKey(upgrade.Slot))
            UnLockedUpgradeParts[upgrade.Slot] = new List<WeaponUpgradeSO>();

        UnLockedUpgradeParts[upgrade.Slot].Add(upgrade);
        return true;
    }
    #endregion

    #region 장착 해제 관리
    public bool ApplyUpgrade(WeaponUpgradeSO upgrade)
    {
        if (!UnLockedUpgradeParts.TryGetValue(upgrade.Slot, out var unlocked)) return false;
        if (!unlocked.Contains(upgrade)) return false;
        _appliedUpgrades[upgrade.Slot] = upgrade;
        return true;
    }
    #endregion
    // 리턴
    // 출력값
    public WeaponInstance GetInstance() => new(BaseData, new List<WeaponUpgradeSO>(_appliedUpgrades.Values));
    public WeaponUpgradeSO GetApplied(UpgradeSlot slot)
    {
        _appliedUpgrades.TryGetValue(slot, out var upgrade);
        return upgrade;
    }
    public bool IsPartUnlocked(WeaponUpgradeSO upgrade)
    {
        if (!UnLockedUpgradeParts.TryGetValue(upgrade.Slot, out var unlockedList))
            return false;
        return unlockedList.Contains(upgrade);
    }

}