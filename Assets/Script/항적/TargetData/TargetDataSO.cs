using UnityEngine;

[CreateAssetMenu(fileName = "NewTargetData", menuName = "Radar/Target Data")]
public class TargetDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string targetName;
    public string exactModelName; // NCTR 식별용 기종명
    
    [Header("Core Logic")]
    public TargetFaction faction; 
    public IFF IFF;
    /*
    
    [Header("Spoofing & Failure")]
    [Range(0, 100)] public float spoofingChance;   // 적이 위장할 확률
    [Range(0, 100)] public float brokenIffChance;  // 아군이 고장 날 확률
    public IFF spoofingIFF;
    public IFF spoofingfaction;
    */
    [Header("Stats")]
    public float speed;
    public float altitude; // 항적의 기준 고도
    [Header("Flight Trajectory")]
    [Tooltip("X축: 0(출발) ~ 1(목표) 진행률 / Y축: 0(0m) ~ 1(설정한  고도) 비율")]
    public AnimationCurve flightCurve;
    public PopUpProfile popUpSettings;
}

public enum TargetFaction { Enemy, Ally, Civilian }
public enum IFF { MIL, CIV, UNKNOW }

[System.Serializable]
public struct PopUpProfile
{
    public bool enablePopUp;              // 팝업 기동 여부
    public float triggerDistance;         // 팝업 개시 남은  거리 (m)
    public float maxPopUpAltitude;        // 팝업 시 도달할 최대 고도 (m)
    public AnimationCurve altitudeCurve;  // [핵심] 고도 변화 궤적 커브 (X: 0~1, Y: 0~1)
}