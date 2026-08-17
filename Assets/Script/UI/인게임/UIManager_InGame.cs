using System;
using UnityEngine;

public class UIManager_InGame : MonoBehaviour
{
    private static UIManager_InGame _instance;
    [Header("UI Sector"), SerializeField]
    private UISectors Sectors;


    [Header("UI Sector_Result [테스트용]")]
    public RoundResultSector LinkedModules;
    private void Awake()
    {
        if(_instance != null)//선점우선
        {
            Destroy(this);
            return;
        }
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    void Start()
    {
        Sectors._Radar.Init(this);
        Sectors._PList.Init(this);
        Sectors._AList.Init(this);
        Sectors._DLink.Init(this);
    }
    public static ActionPack_RadarPingList GetAction_RadarList() => _instance.Sectors._PList.GetActions();
    public static ActionPack_RadarUI GetAction_RadarUI()         => _instance.Sectors._Radar.GetActions();
    public static ActionPack_DL GetActionPack_DL()  => _instance.Sectors._DLink.GetActions();
    public static ActionPack_AMM GetAction_Turret() => _instance.Sectors._AList.GetActions();

    // 라운드 결과 테스트
    public static void Test_RoundResult(Round_Outcome outcome)
    {
        if (_instance == null || _instance.LinkedModules == null)
            return;

        _instance.LinkedModules.gameObject.SetActive(true);
        _instance.LinkedModules.Init(_instance);
        _instance.LinkedModules.Open(outcome, 0); 
        //TODO : 굳이 나눠야할까.. 근데또 통일성생각하면 맞는거같기도하고 .. 이놈은중재자가 아니긴한데
    }
   


}
[Serializable]
public class UISectors
{
    public ArmListSector  _AList;
    public PingListSector _PList;
    public RadarSector    _Radar;
    public DataLinkSector _DLink;
}
[Serializable]
public class LinkedModule
{
    public Radar_Core Radar;
    public Turret_Core Turret;
}

