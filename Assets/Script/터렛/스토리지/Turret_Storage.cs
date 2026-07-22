using UnityEngine;
using System.Collections.Generic;
using System;

public class Turret_Storage : MonoBehaviour
{
    Turret_Core _core;
    [SerializeField] List<MissileStock> missiles; // 이건 테스트용 정상적인방식은아님
    public void Init(Turret_Core core)
    {
        _core = core;
    }
    public void BatchStart()
    {
        SelectedAMM = null;
        foreach (var stock in missiles)
        {
            AddAMMStorage(stock);
        }
    }
    #region 무장제어 구획
    List<MissileStock> StorageAAMList = new();  //시스템에서 관리하는 리스트  
    List<MissileStock> SelectAAMList = new();  //유저가 컨트롤할수있는 리스트 
    // TODO: 무장 시스템 저장/직렬화 구현 완료 시 [SerializeField] 반드시 제거할 것.
    // 유니티 인스펙터가 에디터 내부 캐시(S/N_DIVINA 등)를 강제 주입해서 분탕침.
    [SerializeField]MissileStock SelectedAMM; 

    void AddAMMStorage(MissileStock stock)
    {
        if (stock == null) return;

        StorageAAMList.Add(stock);
        if (ValidAMMSlot(stock)) AddSelectAMMList(stock);              
        
        PlayAction(ValidAction().OnCreateAMM, stock, IsStockSelected(stock));

    }
    void RemoveAMMStorage(MissileStock stock)
    {
        if (stock == null) return;

        if (SelectAAMList.Contains(stock)) RemoveSelectAMMList(stock);// 컨트롤리스트 회수
        StorageAAMList.Remove(stock);

        PlayAction(ValidAction().OnDeleteAMM, stock);
    }
    void AddSelectAMMList(MissileStock stock)
    {
        if (stock == null||SelectAAMList.Contains(stock))  return;
        SelectAAMList.Add(stock);                   
        if(SelectedAMM == null) SelectAMM(stock);
        
    } 
    void RemoveSelectAMMList(MissileStock stock)
    {
        if(SelectedAMM == stock)
        {
            if (SelectAAMList.Count <= 1) SelectAMM(null); 
            else NextSlotAMM(1);
        }
        SelectAAMList.Remove(stock);
    }
    void SelectAMM(MissileStock stock)
    {
        if (SelectedAMM == stock)
        {
            Debug.Log("중복된 선택");
            return;
        }
        SelectedAMM = stock;
    }
    bool ValidAMMSlot(MissileStock stock) => stock.NowCount > 0;
    void NextAMM(int way) //이거근데 오류있음 : 정정 내가 바보였음
    {
        if (SelectAAMList.Count <= 1) return;

        int currentIndex = SelectAAMList.IndexOf(SelectedAMM);
        if (currentIndex == -1) currentIndex = 0;

        int nextIndex = (currentIndex + way + SelectAAMList.Count) % SelectAAMList.Count;
        SelectAMM(SelectAAMList[nextIndex]);
    }

    #endregion

    public MissileStock GetSelectMissleStock() => SelectedAMM;

    public bool IsStockSelected(MissileStock stock) => SelectedAMM == stock;
    public int  GetStockCount() => SelectedAMM.NowCount;
    public void IncreaseMissile(int count) => IncreaseMissile(SelectedAMM, count);
    public bool DecreaseMissile(int count) => DecreaseMissile(SelectedAMM, count);
    // 특정 스톡을 강제로 지정하여 수량을 증감함. (치트, 전체 보급, 시스템 보정 등) 
    public void IncreaseMissile(MissileStock stock, int count)
    {
        stock.NowCount += count;
        if (stock.PeekCount < stock.NowCount) stock.NowCount = stock.PeekCount;
        if (ValidAMMSlot(stock)) AddSelectAMMList(stock);

        PlayAction(ValidAction().OnUpdateAMM, stock, IsStockSelected(stock));
    }
    public bool DecreaseMissile(MissileStock stock, int count) //수량딸리면 걍 리턴때림
    {
        if (stock.NowCount - count < 0) return false;
        stock.NowCount -= count;

        if (!ValidAMMSlot(stock)) RemoveSelectAMMList(stock);

        PlayAction(ValidAction().OnUpdateAMM, stock, IsStockSelected(stock));
        return true;
    }
    public void NextSlotAMM(int way)
    {
        var prev = SelectedAMM;
        NextAMM(way);
        if (prev != null) PlayAction(ValidAction().OnUpdateAMM, prev, IsStockSelected(prev));
        PlayAction(ValidAction().OnUpdateAMM, SelectedAMM, IsStockSelected(SelectedAMM));
    }
    #region 지원 메서드
    private ActionPack_AMM ValidAction()
    {
        var action = _core.GetAction();
        if (!action.IsValid) action = _core.RequestAction();
        return action;
    }
    
    /// <summary>
    /// 액션실행 [삭제]
    /// </summary>
    /// <param name="uiAction">액션 지정</param>
    /// <param name="stock">액션 대상</param>
    private void PlayAction(Action<MissileStock> uiAction, MissileStock stock)
    {
        if (uiAction == null)
        {
            Debug.LogWarning("UI 연결실패 : UI 표시 없이 데이터만 처리합니다.");
            return;
        }
        uiAction.Invoke(stock);
    }
    /// <summary>
    /// 액션실행 [생성, 업데이트]
    /// </summary>
    /// <param name="uiAction">액션 지정</param>
    /// <param name="stock">액션 대상</param>
    /// /// <param name="isSelected">추가 파라미터(선택이된 무장인가)</param>
    private void PlayAction(Action<MissileStock, bool> uiAction, MissileStock stock, bool isSelected)
    {
        if (uiAction == null)
        {
            Debug.LogWarning("UI 연결실패 : UI 표시 없이 데이터만 처리합니다.");
            return;
        }
        uiAction.Invoke(stock, isSelected);
    }
    //제너릭 확장을하면좋을진 모르겠는데 아직은.. 쓸일이없어
   
    #endregion
}