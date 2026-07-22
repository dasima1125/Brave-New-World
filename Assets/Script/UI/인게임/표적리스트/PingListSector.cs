using UnityEngine;
using System.Collections.Generic;
using System;

public class PingListSector : MonoBehaviour
{
    [SerializeField] private Radar_Core radarCore; //TODO:핑 업데이트 부분으로인해 강제적 참조가 필요함
                                                   //차후 레이더시스템에서 호출을통해 업데이트 생성 제거를하되
                                                   //업데이트문같은것이 아닌 이벤트호출(생성 재갱신,근데 tws이게문제네 하)
    
    [Header("UI Templates")]
    [SerializeField] private GameObject pingPanelPrefab; 
    [SerializeField] private Transform contentRoot;
    
    private Dictionary<Ping, PingPanel_AddOn> _activePanels = new();
    UIManager_InGame _core;
    public void Init(UIManager_InGame core) => _core = core;

   
    void FixedUpdate()//반드시 제거해야할구조.
    {
        UpdatePingPanel_ALL(); 
    }

    void AddPingPanel(Ping ping)
    {
        if (ping == null || _activePanels.ContainsKey(ping)) return;
        GameObject panelObj = Instantiate(pingPanelPrefab, contentRoot);
        if (panelObj.TryGetComponent<PingPanel_AddOn>(out var addOnComponent))
        {
            addOnComponent.Render(ping.GetTNS, ping.GetState, false, ping.GetSelect); 
            _activePanels.Add(ping, addOnComponent);
        }
        else Destroy(panelObj);
        
    }
    void RemovePingPanel(Ping ping)
    {
        if (ping == null) return;
        if (_activePanels.TryGetValue(ping, out PingPanel_AddOn addOnComponent))
        {
            Destroy(addOnComponent.gameObject); 
            _activePanels.Remove(ping); 
        }
        
    }
    void UpdatePingPanel_ALL()
    {
        if (_activePanels.Count == 0) return;

        foreach (var pair in _activePanels)
        {
            Ping pingData = pair.Key;
            PingPanel_AddOn view = pair.Value;
            view.Render(pingData.GetTNS, pingData.GetState, radarCore.IsSelectPing(pingData), pingData.GetSelect); 
        }
        
    }

    //접근자
    public ActionPack_RadarPingList GetActions() => new(AddPingPanel,RemovePingPanel);
    
}

public readonly struct ActionPack_RadarPingList
{
    public readonly bool IsValid;
    public readonly Action<Ping> OnPingUIListAdded;
    public readonly Action<Ping> OnPingUIListRemoved;
    public ActionPack_RadarPingList(Action<Ping> onAdded, Action<Ping> onRemoved)
    {
        IsValid             = true;
        OnPingUIListAdded   = onAdded;
        OnPingUIListRemoved = onRemoved;
    }
}