using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradePanel : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private WeaponDataSO weaponData;
    [SerializeField] private UpgradeSlot_AddOn slotEntryPrefab;
    [SerializeField] private Transform contents;

    [Header("스탯")]
    [SerializeField] private TextMeshProUGUI AMMNameText;
    [SerializeField] private TextMeshProUGUI FundsText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI seekerText;
    [SerializeField] private TextMeshProUGUI dlSystemText;

    [Header("버튼")]
    [SerializeField] private Button fireButton;
    [Header("자금")]
    [SerializeField] private int funds;

    private WeaponLoadout _loadout;

    private void Start()
    {
        _loadout = new WeaponLoadout(weaponData);

        // 슬롯 수만큼 SlotEntry 생성
        AMMNameText.text = weaponData.name;
        foreach (var entry in weaponData.AvailableUpgrades)
        {
            var slotEntry = Instantiate(slotEntryPrefab, contents);
            slotEntry.Initialize(_loadout, entry.Slot, RefreshStats, TrySpendFunds);
        }

        // 발사 버튼
        fireButton.onClick.AddListener(OnFireButton);

        // 초기 스탯 표시
        RefreshStats();
        RefreshFunds();
    }

    public void RefreshStats()
    {
        var instance = _loadout.GetInstance();
        speedText.text = $"{instance.ProjectileSpeed}";
        timeText.text = $"{instance.LifeTime}";
        seekerText.text = $"{instance.SeekerType}";
        dlSystemText.text = $"{instance.DLSystem}";
    }
    bool TrySpendFunds(int price)
    {
        if (funds < price) return false;
        funds -= price;
        RefreshFunds();
        return true;
    }
    void RefreshFunds()
    {
        FundsText.text = $"{funds} $";
    }

    private void OnFireButton()
    {
        var instance = _loadout.GetInstance();
        Debug.Log($"발사!\n" +
                  $"Speed: {instance.ProjectileSpeed}\n" +
                  $"LifeTime: {instance.LifeTime}\n" +
                  $"Seeker: {instance.SeekerType}\n" +
                  $"DL: {instance.DLSystem}");
    }
}