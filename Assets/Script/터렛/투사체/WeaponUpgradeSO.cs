using UnityEngine;

public abstract class WeaponUpgradeSO : ScriptableObject
{
    [Header("업그레이드 정보")]
    public string UpgradeName;
    [TextArea] public string Description;
    public UpgradeSlot Slot; // 슬롯종류
    public int Price; // 업그레이드 가격
    

    public abstract void Apply(WeaponInstance instance);

    
}
public enum UpgradeSlot
{
    Wing,       // 날개개선
    Sustain,    // 추진개선
    Fuel,       // 추진체개선
    Seeker,     // 시커교체
    TrackLogic, // 항법교체
    DLSystem    // 데이터링크장착
}
