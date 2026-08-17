using System;
using System.Collections.Generic;
using UnityEngine;



public class Turret_Core : MonoBehaviour
{
    [Header("레이더 모듈")]
    [SerializeField] Radar_Core core;
    [Header("터렛 모듈")]
    [SerializeField] TurretModules _Moudles;
    ActionPack_AMM ActionAMM;
    
    public void Start()
    {
        _Moudles.Storage.Init(this);
        _Moudles.Control.Init(this);

        _Moudles.Storage.BatchStart();

    }
    public ActionPack_AMM GetAction()=> ActionAMM;
    public ActionPack_AMM RequestAction()
    {
        ActionAMM = UIManager_InGame.GetAction_Turret();
        return ActionAMM;
    }
    public void Fire()
    {
        var Stock = _Moudles.Storage.GetSelectMissleStock();
        if(Stock == null || _Moudles.Storage.GetStockCount() <= 0)//선택가능한놈확인 및 수량있나 확인
        {
            if(Stock == null) Debug.Log("선택가능한 무장이없음"); 
            else Debug.Log("수량이딸림");
            return;
        }
        var fireAngle = 360f - (core.GetAllowData().LockBeamAngle - 90f + 360f) % 360f;//레이더한테 내가쏠방향 가져옴 유니티각으로 전환.
        var projectile = _Moudles.Luncher.CreateProjectile(Stock.Spec, transform.position, fireAngle);
        var Success = core.Session_Start(projectile);
      
        Debug.Log("발사성공 여부 : " + Success);
        if(!Success) Destroy(projectile.gameObject); //발사실패시 발사체 제거
        else _Moudles.Storage.DecreaseMissile(1);    //발사성공시 재고감소
        
    }
    public void ChangeAAM(int way)
    {
        if(way == 0) return;
        _Moudles.Storage.NextSlotAMM(way);
    }

}
[Serializable]
public class TurretModules
{
    public Turret_Control Control;
    public Turret_Storage Storage;
    public Turret_Luncher Luncher;
}

[Serializable]
public class MissileStock
{
    public WeaponData Spec;
    public int PeekCount;
    public int NowCount;
}
[Serializable]
public class WeaponData
{
    [Header("미사일 파라미터")]
    public MissileModel MissileModel;
    public Sprite MissileSprite;
    public SeekerType SeekerType;
    public TrackLogicType LogicType;
    public DLMode DLSystem;
    public float ProjectileSpeed;
    public float MaxAngularVelocity;
    public float LifeTime;
    public float NavigationConstant;

}
public enum DLMode
{
    OffLine,
    OnLine,
}
public enum SeekerType
{
    Command,
    SARH,
    ARH,
    IR,
    ARM,      // <== 대방사
}
public enum TrackLogicType
{
    Pure, Lead, Pn
}

public enum MissileModel
{
    S75_DVINA,
    S125_NEVA,
    S200_VEGA
}
