using System;
using System.Collections.Generic;
using UnityEngine;

public class DataLinkSector : MonoBehaviour
{
    
    [SerializeField] UI_SubSector_DL _sub;
    [Header("UI Templates")]
    [SerializeField] private GameObject MTTPanelPrefab; 
    [SerializeField] private Transform contentRoot;
    UIManager_InGame _core; // <-- 사실 이거 필요없긴함 근데.. 역전파를 해야할일도있을수있으니깐?
    public void Init(UIManager_InGame core) => _core = core;
    void OnLineTGT(bool OnOff)     => _sub.TGT.Online(OnOff);
    void OnLineMissile(bool OnOff) => _sub.Missile.Online(OnOff);
    void OnUpdateTGT(STT_TargetData data) => _sub.TGT.UpdateTGT(data);
    void OnUpdateMissile(bool DL, INS2DData INS, TNS2DData TNS) => _sub.Missile.UpdateMissile(true, INS, TNS);
    
    Dictionary<(Ping, Guided), DataLinkAddOn_MTT> _activePanels = new();
    void OnCreatMTTPanel(Ping targetPing, Guided Projectile)
    {
        if (targetPing == null || _activePanels.ContainsKey((targetPing, Projectile))) return;
        GameObject panelObj = Instantiate(MTTPanelPrefab, contentRoot);
        if (panelObj.TryGetComponent<DataLinkAddOn_MTT>(out var addOnComponent))
        {
            bool DL = true; //원래라면.. 가이디드에서뽑아야함 근데 아직 구조고치기전임
            addOnComponent.Render(DL, targetPing, Projectile); 
            _activePanels.Add((targetPing, Projectile), addOnComponent);
        }
        else Destroy(panelObj);
        
    }
    void OnRemoveMTTPanel(Ping targetPing, Guided guided)
    {
        if (targetPing == null) return;
        if (_activePanels.TryGetValue((targetPing, guided), out var panelData))
        {
            Destroy(panelData.gameObject); 
            _activePanels.Remove((targetPing, guided)); 
        }
    }
    void OnUpdateMTTPanel()
    {
        if (_activePanels.Count == 0) return;

        foreach (var pair in _activePanels)
        {
            Ping ping = pair.Key.Item1;
            Guided projectile = pair.Key.Item2;
            bool DL = true;
            pair.Value.Render(DL, ping, projectile);
        }
        
    }
    void FixedUpdate()
    {
        OnUpdateMTTPanel();
    }
    public ActionPack_DL GetActions()=> new(OnLineTGT, OnLineMissile, OnUpdateTGT, OnUpdateMissile, OnCreatMTTPanel, OnRemoveMTTPanel);

}
[Serializable]
public class UI_SubSector_DL
{
    public DataLinkAddOn_STT TGT;
    public DataLinkAddOn_STT Missile;
        
}
public readonly struct ActionPack_DL
{ 
    public readonly bool IsValid;
    public readonly Action<bool> OnLineTGT;
    public readonly Action<bool> OnLineMissile;
    public readonly Action<STT_TargetData> OnUpdateTGT;
    public readonly Action<bool, INS2DData, TNS2DData> OnUpdateMissile;

    public readonly Action<Ping, Guided> OnCreatMTTPanel;
    public readonly Action<Ping, Guided> OnRemoveMTTPanel;

    public ActionPack_DL(
        Action<bool> P_TGT, 
        Action<bool> P_Missile, 
        Action<STT_TargetData> P_UpdateTGT, 
        Action<bool, INS2DData, TNS2DData> P_UpdateMissile,

        Action<Ping, Guided> P_CreateMTTPanel,
        Action<Ping, Guided> P_RemoveMTTPanel
    )
    {
        IsValid = true;
        OnLineTGT = P_TGT;
        OnLineMissile = P_Missile;
        OnUpdateTGT = P_UpdateTGT;
        OnUpdateMissile = P_UpdateMissile;
        
        OnCreatMTTPanel = P_CreateMTTPanel;
        OnRemoveMTTPanel = P_RemoveMTTPanel;
    }

    
}
