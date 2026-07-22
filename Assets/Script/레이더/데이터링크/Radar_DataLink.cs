using UnityEngine;
using System.Collections.Generic;
using System;

public class Radar_DataLink : MonoBehaviour
{
    [SerializeField] private GameObject Ping_Trace;
    Radar_Core core;
    Radar_AllowRangeSet _allowRangeSet = new();
    STT_TargetData _targetData = new();
    public void Init(Radar_Core core) => this.core = core;

    #region 레이더 허용범위 인증
    public void SetAllowRange(float maxRange, float angle, float arc)
    {
        _allowRangeSet.MaxLockRange = maxRange;
        _allowRangeSet.LockBeamAngle = angle;
        _allowRangeSet.LockBeamArc = arc;
    }
    public STT_TargetData GetSTTTargetData() => _targetData;
    #endregion
    #region 이벤트 관리
    ActionPack_DL actionPack_DL;

    private ActionPack_DL ValidAction()// 검증자
    {
        var action = actionPack_DL;
        if (!action.IsValid) action = UIManager_InGame.GetActionPack_DL();
        return action;
    }
    private void PlayAction(Action<bool> uiAction, bool type) // 온라인 오프라인 확인
    {
        if (uiAction == null)
        {
            Debug.LogWarning("UI 연결실패");
            return;
        }
        uiAction.Invoke(type);
    }
    private void PlayAction(Action<STT_TargetData> uiAction, STT_TargetData data) // STT 데이터 업데이트
    {
        if (uiAction == null)
        {
            Debug.LogWarning("UI 연결실패");
            return;
        }
        uiAction.Invoke(data);
    }
    private void PlayAction(Action<bool, INS2DData, TNS2DData> uiAction, INS2DData INS, TNS2DData TNS, bool DL) // 미사일 데이터 업데이트
    {
        if (uiAction == null)
        {
            Debug.LogWarning("UI 연결실패");
            return;
        }
        uiAction.Invoke(DL, INS, TNS);
    }
    private void PlayAction(Action<Ping, Guided> uiAction, Ping targetPing, Guided projectile) // MTT 데이터 갱신
    {
        if (uiAction == null)
        {
            Debug.LogWarning("UI 연결실패");
            return;
        }
        uiAction.Invoke(targetPing, projectile);
    }
    void STT_Online(bool type) => PlayAction(ValidAction().OnLineTGT, type);
    void Missile_Online(bool type) => PlayAction(ValidAction().OnLineMissile, type);
    void UpdateSTT(STT_TargetData data) => PlayAction(ValidAction().OnUpdateTGT, data);
    void UpdateMissile(bool DL, INS2DData INS, TNS2DData TNS) => PlayAction(ValidAction().OnUpdateMissile, INS, TNS, DL);
    void CreatMTTPanel(Ping targetPing, Guided projectile) => PlayAction(ValidAction().OnCreatMTTPanel, targetPing, projectile);
    void RemoveMTTPanel(Ping targetPing, Guided projectile) => PlayAction(ValidAction().OnRemoveMTTPanel, targetPing, projectile);
    #endregion

    //여기부터 진짜 데이터링크 시스템

    #region 데이터링크 - 통합 관리
    [SerializeField] private Upgrade_MultiEngagementSystem MultiEngagementUpgrade;
    [SerializeField] private int Upgrade_MultiEngagementsCount;
    private List<TargetSessionDL> Trace_Sessions = new();
    public void SetUpgrade_MultiEngagementLvl(Upgrade_MultiEngagementSystem upgrade) => MultiEngagementUpgrade = upgrade;
    public void SetUpgrade_MultiEngagementCount(int count) => Upgrade_MultiEngagementsCount = count;
    public (Upgrade_MultiEngagementSystem upgrade, int count) GetUpgrade_MultiEngagement() => (MultiEngagementUpgrade, Upgrade_MultiEngagementsCount);
    void FixedUpdate()
    {
        STT_LifeCycle();
        MTT_LifeCycle();
    }

    #endregion
    #region STT 관리 
    List<TargetSessionDL> STT_Sessions = new();
    TNS2DData STT_TNS;
    bool _hasSTT;
    void STT_LifeCycle()
    {
        for (int i = STT_Sessions.Count - 1; i >= 0; i--)
        {
            var session = STT_Sessions[i];
            if (session == null || !SessionAllowCheck(session.projectile))
            {
                Session_Terminate(session);
                continue;
            }
            if (STT_TNS.InVaild) session.projectile.ReceiveCommand(STT_TNS);
            var INS = session.projectile.GetINS();
            if (session.ProjectilePing != null)
                session.ProjectilePing.MovePing(INS.position, INS.rotation);

            if (i == STT_Sessions.Count - 1) //한개라도있을시 마지막꺼 갱신
                UpdateMissile(true, INS, session.projectile.GetTNS());
        }

    }
    void RegisterSTT(TargetSessionDL session)
    {
        STT_Sessions.Add(session);
        if (STT_Sessions.Count == 1) Missile_Online(true);
    }
    void UnregisterSTT(TargetSessionDL session)
    {
        if (session.ProjectilePing) session.ProjectilePing.DestroyPing();
        STT_Sessions.Remove(session);

        if (STT_Sessions.Count == 0) Missile_Online(false);
    }
    public void SetSTTTargetData(float angle, float dist, Vector2 vel, bool isValid)
    {
        _targetData.TargetAngle = angle;
        _targetData.TargetDistance = dist;
        _targetData.Velocity = vel;
        // STT 유효성검증 일단 접근부가여기임
        if (_hasSTT != isValid)
        {
            _hasSTT = isValid;
            STT_Online(isValid);
        }
        if (_hasSTT) UpdateSTT(_targetData);
    }
    public void UpdateTNS_Global(Rigidbody2D rb) => STT_TNS = (rb != null) ? new TNS2DData(rb) : default;
    public Radar_AllowRangeSet GetAllowRange() => _allowRangeSet;

    #endregion
    #region MTT 관리
    List<(Ping Target, TargetSessionDL Session)> MTT_Sessions = new();
    void MTT_LifeCycle()
    {
        for (int i = MTT_Sessions.Count - 1; i >= 0; i--)
        {
            var (Target, Session) = MTT_Sessions[i];
            if (!Target || Target.GetState != PingState.SRC_TWS)
            {
                Session_Terminate(Session);
                Debug.Log("타겟 검증실패");
                continue;
            }
            if (Session == null || !SessionAllowCheck(Session.projectile))
            {
                Session_Terminate(Session);
                Debug.Log("투사체 검증실패");
                continue;
            }
            TNS2DData TNS = new(Target.transform.position, Target.GetTNS.velocity);
            Session.projectile.ReceiveCommand(TNS);

            var INS = Session.projectile.GetINS(); // 이거 메서드화시켜도될려나?
            if (Session.ProjectilePing != null)
                Session.ProjectilePing.MovePing(INS.position, INS.rotation);
        
        }

    }
    void RegisterMTT(Ping TargetPing, TargetSessionDL session)
    {
        MTT_Sessions.Add((TargetPing, session));
        CreatMTTPanel(TargetPing, session.projectile); // MTT는 아직 오프라인 온라인 UI없음
    }
    void UnregisterMTT(TargetSessionDL session)
    {
        for (int i = 0; i < MTT_Sessions.Count; i++)
        {
            if (MTT_Sessions[i].Session == session)
            {
                Debug.Log("세션해제");
                var (Target, Session) = MTT_Sessions[i];

                MTT_Sessions.RemoveAt(i);
                RemoveMTTPanel(Target, Session.projectile);
                if (Session.ProjectilePing) Session.ProjectilePing.DestroyPing();
                return;
            }
        }

    }

    #endregion
    #region 세션 관리 및 처분
    public bool Session_Start(Guided projectile, Ping target = null)
    {

        if (!VerifyConditions(projectile, out bool TracePing)) return false;
        var session = new TargetSessionDL
        {
            projectile = projectile,
            ProjectilePing = TracePing ? Instantiate(Ping_Trace).GetComponent<Ping_Trace>() : null
        };
        session.projectile.UploadLink(() => Session_Terminate(session));

        if (target == null) RegisterSTT(session);
        else RegisterMTT(target, session);

        Trace_Sessions.Add(session);
        if (Trace_Sessions.Count > Upgrade_MultiEngagementsCount) Session_Terminate(Trace_Sessions[0]);

        return true;
    }
    void Session_Terminate(TargetSessionDL session)
    {
        if (Trace_Sessions.Contains(session)) Trace_Sessions.Remove(session);
        if (STT_Sessions.Contains(session)) UnregisterSTT(session);
        else UnregisterMTT(session);
    }
    bool SessionAllowCheck(Guided target) //STT_Verfiy와 똑같은구조  모노레이더모드에서 문제발생 개선요망
    {
        var allow = core.GetAllowData();
        Vector2 targetPos = target.transform.position;
        Vector2 myPos = transform.position;
        Vector2 dir = targetPos - myPos;

        float distance = dir.magnitude;
        if (distance < 1.0f) return true;
        if (distance > allow.MaxLockRange)
        {
            Debug.Log("거리탈출");
            return false;
        }

        float targetAngle = 360f - (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f + 360f) % 360f;
        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(allow.LockBeamAngle, targetAngle));
        if (angleDiff > (allow.LockBeamArc / 2f)) // 문제발견 모노레이더모드에서 발생 pn 이나 리드에서문제발생 각을 너무잘 나가짐 이유가뭐지?
        {
            Debug.Log("현재각 : " + angleDiff + "검사각 : " + allow.LockBeamArc / 2f);
            Debug.Log("각도탈출");
            return false;
        }

        return true;
    }
    bool VerifyConditions(Guided projectile, out bool tracePing)
    {
        tracePing = false;
        var seekerType = projectile.GetSeekerType();
        var dl = projectile.GetDLMode();

        switch (seekerType)
        {
            case SeekerType.Command:
                tracePing = true;
                return true;

            case SeekerType.SARH:
                if (!_hasSTT) return false; // 하드락 필수
                if (MultiEngagementUpgrade == Upgrade_MultiEngagementSystem.Stage_1) return false;
                if (MultiEngagementUpgrade == Upgrade_MultiEngagementSystem.Stage_2) return true;
                if (MultiEngagementUpgrade == Upgrade_MultiEngagementSystem.Stage_3)
                {
                    if (dl == DLMode.OnLine) tracePing = true;
                    return true; // 성공시 리턴
                }
                return false;

            case SeekerType.ARH:
                if (MultiEngagementUpgrade == Upgrade_MultiEngagementSystem.Stage_1) return false;
                if (MultiEngagementUpgrade == Upgrade_MultiEngagementSystem.Stage_2)
                {
                    if (!_hasSTT) return false; // 스테이지2에선 소프트락 TWS 발사 제한
                    return true;
                }
                if (MultiEngagementUpgrade == Upgrade_MultiEngagementSystem.Stage_3)
                {
                    if (dl == DLMode.OnLine) tracePing = true;
                    return true;
                }
                return false;

            case SeekerType.IR:
                // TODO: 열추적을만들일이 근데있을려나?
                return true;

            default:
                return false;
        }
    }
    public void ShotDonwProtocall()
    {
        _allowRangeSet = new();
        _targetData = new();

        ClearSTT();
        ClearMTT();
        if (_hasSTT)
        {
            _hasSTT = false;
            STT_Online(false);
        }
    }
    public void ClearSTT()
    {
        for (int i = STT_Sessions.Count - 1; i >= 0; i--) Session_Terminate(STT_Sessions[i]);
    }
    public void ClearMTT()
    {
        for (int i = MTT_Sessions.Count - 1; i >= 0; i--) Session_Terminate(MTT_Sessions[i].Session);
    }
    #endregion
}
[Serializable]
public class TargetSessionDL
{
    public Ping_Trace ProjectilePing; // 미사일 핑 참조
    public Guided projectile;   // 투사체참조
}

[Serializable]
public class Radar_AllowRangeSet
{
    public float MaxLockRange;
    public float LockBeamAngle;
    public float LockBeamArc;
}
[Serializable]
public class STT_TargetData
{
    public float TargetAngle;
    public float TargetDistance;
    public Vector2 Velocity;
}
public enum Upgrade_MultiEngagementSystem
{
    Stage_1,      // 지령, IR 이둘은 모든 스테이지에서 가능 IR은 항상 추적안함
    Stage_2,      // STT SARH, ARH 사용가능
    Stage_3       // MTT ARH 사용가능 또한 STT시 ARH SARH 미사일 추적가능
}
