using System;
using UnityEngine;

public class RadarSector : MonoBehaviour
{
    UIManager_InGame _core;
    [SerializeField] UI_SubSector_Radar _Sub;
    public void Init(UIManager_InGame core) => _core = core;
    void PowerOnOff(bool OnOff) => _Sub.RadarPanel.SetActive(OnOff);
    void Update_SRC(string type, string width, string range)
    {
        _Sub.SRCPanel.Render_SRC(type, width, range);
    }
    void Update_TRK(string type)
    {
        _Sub.TRKPanel.Render_TRK(type);
    }
    //void OnUpdateSRC


    public ActionPack_RadarUI GetActions()=> new(PowerOnOff,Update_SRC,Update_TRK);
}
[Serializable]
public class UI_SubSector_Radar
{
    public GameObject RadarPanel;
    public RadarPanel_AddOn SRCPanel;
    public RadarPanel_AddOn TRKPanel;
}

public readonly struct ActionPack_RadarUI
{ 
    public readonly bool IsValid;
    public readonly Action<bool> OnOff;
    public readonly Action<string, string, string> UpdateSRC;
    public readonly Action<string> UpdateTRK;
    //public readonly Action OnUpdateSRC;
    //public readonly Action OnUpdateTRK;
    public ActionPack_RadarUI(Action<bool> Power, Action<string, string, string> src, Action<string> trk)
    {
        IsValid = true;
        OnOff   = Power;
        UpdateSRC = src;
        UpdateTRK = trk;
    }

    
}

