using System.Collections.Generic;
using UnityEngine;
using System;

public class ArmListSector : MonoBehaviour
{
    
    [Header("UI Templates")]
    [SerializeField] private GameObject AAMPanelPrefab;
    [SerializeField] private Transform contentRoot;
    Dictionary<MissileStock, AAMPanel_AddOn> _activePanels = new();
    UIManager_InGame _core;
    public void Init(UIManager_InGame core) => _core = core;
    public void CreateAMMPanel(MissileStock stock, bool isSelect)
    {
        if (stock == null) return;
        GameObject panelObj = Instantiate(AAMPanelPrefab, contentRoot);
        if (panelObj.TryGetComponent<AAMPanel_AddOn>(out var addOn))
        {
            addOn.Render(stock,isSelect);
            _activePanels.Add(stock, addOn);
        }
        else Destroy(panelObj);

    }
    public void DeleteAMMPanel(MissileStock stock)
    {
        if (stock == null) return;
        if (_activePanels.TryGetValue(stock, out var targetPanel))
        {
            if (targetPanel != null) Destroy(targetPanel.gameObject);
            _activePanels.Remove(stock);
        }
    }
    public void UpdateAMMPanel(MissileStock stock, bool isSelect)
    {
        var target = _activePanels[stock];
        target.Render(stock,isSelect);
    }
    public ActionPack_AMM GetActions() => new(CreateAMMPanel, DeleteAMMPanel, UpdateAMMPanel);
}
/// <summary>
/// 터렛 시스템과 인게임 모듈 간의 신호 전송용 액션 패키지입니다.
/// <para> OnCreateAMM : [UI 생성] 무장고 기반 새 패널 생성</para>
/// <para> OnDeleteAMM : [UI 삭제] 무장 유실 시 슬롯 삭제</para>
/// <para> OnUpdateAMM : [UI 갱신] 발사 후 수량 실시간 동기화</para>
/// </summary>
public readonly struct ActionPack_AMM
{

    public readonly bool IsValid;
    public readonly Action<MissileStock, bool> OnCreateAMM;
    public readonly Action<MissileStock> OnDeleteAMM;
    public readonly Action<MissileStock, bool> OnUpdateAMM;
    public ActionPack_AMM(Action<MissileStock, bool> OnCreate, Action<MissileStock> OnDelete, Action<MissileStock, bool> OnUpdate)
    {
        IsValid = true;
        OnCreateAMM = OnCreate;
        OnDeleteAMM = OnDelete;
        OnUpdateAMM = OnUpdate;
    }
}
