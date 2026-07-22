using UnityEngine;
using TMPro;
using System;

public class UpgradeSlot_AddOn : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI slotNameText;
    [SerializeField] private TextMeshProUGUI nowPartsText;
    [SerializeField] private DropDown_Custom DropDown;


    [SerializeField] private WeaponLoadout _loadout; // 로드아웃 참조
    [SerializeField] private UpgradeSlot _slot;      // 로드아웃 종류
    private Action OnUpgradeChanged; 
    private Func<int, bool> OnSpend; // 리턴필요함
    public void Initialize(WeaponLoadout loadout, UpgradeSlot slot, Action OnUpgrade, Func<int, bool> OnSpend)
    {
        _loadout = loadout;
        _slot = slot;

        slotNameText.text = slot.ToString();
        OnUpgradeChanged += OnUpgrade;
        this.OnSpend += OnSpend;

        UpdateNowPartsText();
    }

    public void OpenPartsPanel()
    {
        var parts = _loadout.AllUpgradeParts[_slot];
        DropDown.OpenDropdown(parts.Count);

        var items = DropDown.GetItemComponents<UpgradeSlotDD_AddOn>();
        // 람다식은 i 변수 자체를 참조로 캡처
        // 루프가 진행될때마다 모든 람다의 i값이 현재 i로 갱신됨
        // 예) i=0 등록 → i=1 등록시 이전 람다도 i=1로 변경
        // 모든 요소가 마지막 인덱스로 정해짐
        // 루프 종료시 i=items.Count → 모든 람다가 범위 초과 인덱스 참조
        // 버튼 클릭 시점에 items[items.Count] → ArgumentOutOfRangeException
        /*for (int i = 0; i < items.Count; i++) 
        {
            var part = parts[i];
            bool isUnlocked = _loadout.UnLockedUpgradeParts.TryGetValue(_slot, out var unlocked) && unlocked.Contains(part);
            items[i].Render(part,
                            isUnlocked,
                            onUnlock: (target) => { if (BuyParts(target)) items[i].Render(target, true, null, (onSelect) => PartSelect(onSelect)); },
                            onSelect: (target) => PartSelect(target));
                            //onUnlock :  buyParts가 부울이여서 결과 반환  성공시 해당인덱스 재 랜더링 랜더시 액션을 다시넣어줘야함.
        }
        */
        for (int i = 0; i < items.Count; i++)
        {
            var index = i; // 캡처용 복사본
            var part = parts[i];
            bool isUnlocked = _loadout.UnLockedUpgradeParts.TryGetValue(_slot, out var unlocked) && unlocked.Contains(part);
            items[index].Render(part,
                            isUnlocked,
                            onUnlock: (target) => { if (BuyParts(target)) items[index].Render(target, true, null, (onSelect) => PartSelect(onSelect)); },
                            onSelect: (target) => PartSelect(target));
        }

    }
    bool PartSelect(WeaponUpgradeSO part)
    {

        if (!_loadout.ApplyUpgrade(part))
        {
            Debug.Log("교체 실패");
            return false;
        }

        UpdateNowPartsText();
        OnUpgradeChanged?.Invoke();
        return true;
    }
    bool BuyParts(WeaponUpgradeSO part)
    {
        if (!OnSpend.Invoke(part.Price))
        {
            Debug.Log("언락 실패");
            return false;
        }
        _loadout.Unlock(part);
        return true;
    }


    void UpdateNowPartsText()
    {
        var applied = _loadout.GetApplied(_slot);
        if (applied != null)
            nowPartsText.text = applied.UpgradeName;
    }


}