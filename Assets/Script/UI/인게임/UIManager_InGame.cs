using System;
using UnityEngine;

public class UIManager_InGame : MonoBehaviour
{
    private static UIManager_InGame _instance;
    [Header("UI Sector"), SerializeField]
    private UISectors Sectors;
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
    public static ActionPack_RadarUI GetAction_RadarUI()  => _instance.Sectors._Radar.GetActions();
    public static ActionPack_DL GetActionPack_DL() => _instance.Sectors._DLink.GetActions();
    public static ActionPack_AMM GetAction_Turret()          => _instance.Sectors._AList.GetActions();
   


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

