using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UpgradeSlotDD_AddOn : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI partNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button PanelButton;
    [SerializeField] private Button unlockButton;
    
    public void Render(WeaponUpgradeSO part, 
                        bool isUnlocked, 
                        Action<WeaponUpgradeSO> onUnlock, 
                        Action<WeaponUpgradeSO> onSelect)
    {
        if (unlockButton == null || priceText == null) return;

        partNameText.text = part.UpgradeName;
        PanelButton.onClick.RemoveAllListeners();
        PanelButton.onClick.AddListener(() => onSelect?.Invoke(part));

        if (isUnlocked)
        {
            ShowAddOn(false);
        }
        else
        {
            ShowAddOn(true);
            priceText.text = $"{part.Price} $";     // 가격 기입
            
            // 버튼 클릭 리스너 바인딩
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(() => 
            {
                onUnlock?.Invoke(part);
            });
        }
    }
    void ShowAddOn(bool show)
    {
        unlockButton.gameObject.SetActive(show); 
        priceText.gameObject.SetActive(show);   
    }
}
