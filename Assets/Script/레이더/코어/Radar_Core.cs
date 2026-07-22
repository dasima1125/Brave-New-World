using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class Radar_Core : MonoBehaviour
{
    public RadarSystem_Type RadarSystem_Type;
    [SerializeField] LineLenderTest TrackLine;
    [SerializeField] RadarModules _Modules;

    void Start()
    {
        _Modules.Search.Init(this);
        _Modules.Track.Init(this);
        _Modules.Control.Init(this);
        _Modules.DataLink.Init(this);
    }



    #region 핑 메모리
    Dictionary<Signal, Ping> signalMap = new();
    void RegisterNewPing(Signal target, Ping ping) => signalMap.Add(target, ping);
    void RemoveDictPing(Signal target) => signalMap.Remove(target);
    public bool TryGetExistingPing(Signal target, Collider2D hit, DetectionType type, out Ping ping)
    {
        if (type == DetectionType.SRC) SRCCurrentFrame.Add(hit);
        else TRKCurrentFrame.Add(hit);
        return signalMap.TryGetValue(target, out ping);
    }
    HashSet<Collider2D> SRCLastFrame = new();
    HashSet<Collider2D> SRCCurrentFrame = new();
    public bool SRC_WasDetectedLastFrame(Collider2D hit) => SRCLastFrame.Contains(hit);
    public void SRC_SwapDetectionFrames() => (SRCCurrentFrame, SRCLastFrame) = (SRCLastFrame, SRCCurrentFrame);
    public void SRC_ClearDetection() => SRCCurrentFrame.Clear();
    HashSet<Collider2D> TRKLastFrame = new();
    HashSet<Collider2D> TRKCurrentFrame = new();
    public bool TRK_WasDetectedLastFrame(Collider2D hit) => TRKLastFrame.Contains(hit);
    public void TRK_SwapDetectionFrames() => (TRKCurrentFrame, TRKLastFrame) = (TRKLastFrame, TRKCurrentFrame);
    public void TRK_ClearDetection() => TRKCurrentFrame.Clear();
    #endregion

    #region 핑 제어 메모리 구획

    [SerializeField] List<Ping> OrignalPingList = new(); //시스템에서 관리하는 전체 핑 
    [SerializeField] List<Ping> SelectPingList = new();  //유저가 컨트롤할수있는 리스트 
    [SerializeField] private Transform pingPrefab;       //핑 원본 프리팹
    [SerializeField] float pingLiveTime = 0f;
    private Ping SelectedPing;         //선택된 핑
    private Ping HardLockPing;         //하드락 전용
    private long PIDStack = 1;         //핑 고유아이디 생성용 스택

    // --- 등록 및 생명주기 ---
    void RegisterPing(Signal target, Ping ping)
    {
        RegisterNewPing(target, ping);
        RegisterOriginalPingList(ping);
        //액션주입
        ping.ActionInjector(() =>
        {
            RemoveDictPing(target);
            UnregisterOriginalPing(ping);
        });

    }
    void RegisterOriginalPingList(Ping ping)
    {
        OrignalPingList.Add(ping);
        if (OnPingUIListAdded == null) RequestUiActions();
        OnPingUIListAdded?.Invoke(ping);

        switch (RadarSystem_Type)
        {
            case RadarSystem_Type.MonoRadar:
                SelectPingList.Add(ping);
                //이구조도 메서드화를시킬까?.. 근데 굳이긴하지만
                if (SelectedPing == null)
                {
                    SelectedPing = ping;
                    SelectedPing.SetSelect(true);
                }
                break;
            case RadarSystem_Type.MultiRadar:
                if (!VaildSelectedPing(ping)) break;
                SelectPingList.Add(ping);
                if (SelectedPing == null)
                {
                    SelectedPing = ping;
                    SelectedPing.SetSelect(true);
                }
                break;
        }
    }
    void UnregisterOriginalPing(Ping ping)
    {
        if (SelectPingList.Contains(ping)) RemoveSelectListPing(ping);
        OrignalPingList.Remove(ping);
        if (OnPingUIListRemoved == null) RequestUiActions();
        OnPingUIListRemoved?.Invoke(ping);
    }
    // --- 검증 및 동기화 ---
    void VaildSelectedPings()
    {
        //검증주기는 아마 이게지금 탐색레이더가 호출중인데 별도 레이더 코어모듈부근이해야함 
        // 이 검증로직이 사실상지금 탐색에 종속됨

        if (RadarSystem_Type == RadarSystem_Type.MonoRadar) return;
        for (int i = SelectPingList.Count - 1; i >= 0; i--)
        {
            Ping targetPing = SelectPingList[i];
            if (!VaildSelectedPing(targetPing))
            {
                targetPing.SetState(PingState.SRC_Nomarl);
                RemoveSelectListPing(targetPing);
            }

        }
        foreach (var p in OrignalPingList)
        {
            if (SelectPingList.Contains(p)) continue;
            if (VaildSelectedPing(p))
            {
                SelectPingList.Add(p);
                if (SelectedPing == null)
                {
                    SelectedPing = p;
                    SelectedPing.SetSelect(true);
                }
            }
        }
    }
    bool VaildSelectedPing(Ping ping)
    {
        var a = _Modules.DataLink.GetAllowRange();
        return ping.GetTNS.RDistance <= a.MaxLockRange && Mathf.Abs(Mathf.DeltaAngle(a.LockBeamAngle, ping.GetTNS.RAngle)) <= a.LockBeamArc / 2f;
    }
    // --- 제어 ---
    void ChangeSelectedPing(int way)
    {
        int count = SelectPingList.Count;

        if (count <= 1) return;

        SelectedPing.SetSelect(false);
        NextSelectedPing(SelectPingList.IndexOf(SelectedPing), count, way);
    }
    void NextSelectedPing(int index, int length, int way)
    {
        SelectedPing = SelectPingList[(index + way + length) % length];
        SelectedPing.SetSelect(true);
    }
    // --- 제거 ---
    void RemoveSelectListPing(Ping targetPing)
    {
        if (SelectedPing == targetPing)
        {
            SelectedPing.SetSelect(false);
            if (SelectPingList.Count <= 1) SelectedPing = null;
            else NextSelectedPing(SelectPingList.IndexOf(targetPing), SelectPingList.Count, 1);
        }
        SelectPingList.Remove(targetPing);
    }
    void PingClear()
    {
        for (int i = OrignalPingList.Count - 1; i >= 0; i--)
            if (OrignalPingList[i] != null) OrignalPingList[i].KillPing();
    }
    public void ClearTWSPings()
    {
        foreach (var ping in SelectPingList)
        {
            if (ping.GetState == PingState.SRC_TWS) ping.SetState(PingState.SRC_Nomarl);
        }
    }
    // --- 데이터 제공 ---
    public Ping GetSelectPing => SelectedPing; // 제거예정
    public bool IsOrignalPing(Ping ping) => OrignalPingList.Contains(ping);
    public bool IsSelectPing(Ping ping) => SelectPingList.Contains(ping);
    public bool IsHardLocked(Ping ping) => HardLockPing == ping;
    // --- 팩토리 ---
    public Ping PingCreate(Signal target, float angle, float dist, PingState state = PingState.SRC_Nomarl)
    {
        Ping newPing = Instantiate(pingPrefab, target.transform.position, Quaternion.identity).GetComponent<Ping>();
        PingTNS PNS = new(
                    angle: angle,
                    distance: dist,
                    pos: target.transform.position
        );
        newPing.Init(PIDStack++, transform.position, PNS, pingLiveTime);
        newPing.SetState(state);
        RegisterPing(target, newPing);

        return newPing;
    }
    //TODO: 너무 더럽다 나중에 리전으로 생성,선택,나머지 이런식으로 배치좀바꿀까?
    #endregion

    public float CurrentBeamAngle; // <-- 디버깅용
    [SerializeField] private RadarState _state = RadarState.SRC; // <-- 디버깅용
    private Coroutine _orderAction;
    private Coroutine _STTLoopAction;
    private bool _Power = true;

    #region 시퀀스: 통합

    // 입력제어부
    public void MoveBeamAngle(float way) => _Modules.Control.MoveHeadonbeam_Ctrl(way);
    public void ChangePing(int way) => ChangeSelectedPing(way);
    public void HardLock()
    {
        if (RadarSystem_Type == RadarSystem_Type.MonoRadar)
        {
            if (!SelectedPing)
            {
                Debug.Log("되겠냐?");
                return;
            }
            Mono_ACQ();
        }
        
        if (RadarSystem_Type == RadarSystem_Type.MultiRadar) SRC_TRK_ACQ();
    }
    public void Power_TWS() => _Modules.Track.ToggleTWSMode();
    public void Power_Radar()
    {
        _Power = !_Power;
        _actionUi.OnOff?.Invoke(_Power);
        if (_Power)// 작동시 부울문활성화 및 작동실행 명령
        {
            _Modules.Search.gameObject.SetActive(_Power);
            _Modules.Track.gameObject.SetActive(_Power);
            SetState(default);

        }
        else//종료시 시스템 종료 및 모든 저장장로직 초기화
        {
            if (_orderAction != null)
            {
                StopCoroutine(_orderAction);
                _orderAction = null;
            }
            SetState(default);
            

            //2. 모듈기능철수및 종료
            _Modules.Search.gameObject.SetActive(_Power);
            _Modules.Track.gameObject.SetActive(_Power);
            _Modules.DataLink.ShotDonwProtocall();
            PingClear();
        }
    }
    // end
    public void UpdateBeamAngle(float angle) => CurrentBeamAngle = angle;
    public RadarState GetState() => _state;
    void SetState(RadarState state)
    {

        _state = state;
        //상태업데이트는 이곳을통해함 즉 여기서 하면될려나? 물론끄는건제외
        //액션을 쓸떄 그냥인보크가아닌 항시 검증을 먼저해줘야함 
        //ex)
        /*
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
        */
        
        렌더러고민용전송();
        return;
        

    }
    void 렌더러고민용전송()
    {
        if(RadarSystem_Type == RadarSystem_Type.MonoRadar)
        switch (_state) //개판같다 그죠?
        {
            case RadarState.SRC:
                _actionUi.UpdateSRC?.Invoke(_state.ToString(), "360° x 45°", "150 Km*");
                break;
            case RadarState.ACQ:
                _actionUi.UpdateSRC?.Invoke(_state.ToString(), "10° x 10°", null);
                break;
            case RadarState.STT_TRK:
                _actionUi.UpdateSRC?.Invoke(_state.ToString(),null, "150 Km*");
                break;
        }
        else
        {
            switch (_state) //개판같다 그죠?
            {
                case RadarState.SRC:
                    _actionUi.UpdateTRK?.Invoke("");
                    break;
                case RadarState.ACQ:
                    _actionUi.UpdateTRK?.Invoke("ACQ");
                    break;
                case RadarState.STT_TRK:
                    _actionUi.UpdateTRK?.Invoke("TRK");
                    break;
            }
        }
        
    }

    // --- STT관련 ---
    void STTLock_Start(Signal sig, Ping ping)
    {
        if (_STTLoopAction != null) return;
        SetState(RadarState.STT_TRK);
        
        TrackLine.Cupuling(ping.transform);
        HardLockPing = ping;

        _Modules.DataLink.ClearMTT();
        _STTLoopAction = StartCoroutine(STTLoop(sig, ping, () => STTLock_Terminate(ping)));
    }
    void STTLock_Terminate(Ping ping)
    {
        SetState(RadarState.SRC);
        TrackLine.Decupuling();
        
        HardLockPing = null;
        _STTLoopAction = null;
        
        if(!_Power) return; // 파워가 죽을땐 핑자체를 다 날림 즉 핑을 관리할이유가없음
        
        switch (RadarSystem_Type)
        {
            case RadarSystem_Type.MonoRadar:
                ping.KillPing();
                break;
            case RadarSystem_Type.MultiRadar:
                ping.SetState(PingState.SRC_Nomarl);
                break;
        }
    }
    bool STTLock_Verify(Signal target)
    {
        if (target == null) return false;

        var checkData = _Modules.DataLink.GetAllowRange();

        //1차 거리계산
        Vector2 targetPos = target.transform.position;
        Vector2 myPos = transform.position;
        Vector2 directionToTarget = targetPos - myPos;

        float distance = directionToTarget.magnitude;
        if (distance > checkData.MaxLockRange) return false;

        float targetAngle = 360f - (Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f + 360f) % 360f;
        //2차 각도  검사 
        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(checkData.LockBeamAngle, targetAngle));
        if (angleDiff > (checkData.LockBeamArc / 2f)) return false;

        return true;

    }
    IEnumerator STTLoop(Signal target, Ping targetPing, Action onTerminate)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (SelectedPing != null && SelectedPing != targetPing)
        {
            SelectedPing.SetSelect(false);
            SelectedPing = targetPing;
            SelectedPing.SetSelect(true);
        }
        try
        {
            while (GetState() == RadarState.STT_TRK)
            {
                // 탈락 조건 검사
                if (target == null || !STTLock_Verify(target)) break;

                Vector2 dir = (Vector2)target.transform.position - (Vector2)transform.position;
                float liveAngle = 360f - (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f + 360f) % 360f;
                float liveDist = dir.magnitude;
                targetPing.transform.position = target.transform.position;
                PingTNS PNS = new(
                    angle: liveAngle,
                    distance: liveDist,
                    pos: target.transform.position,
                    vel: targetRb.linearVelocity
                );
                targetPing.RxUpdate(transform.position, PNS);

                _Modules.DataLink.SetSTTTargetData(liveAngle, liveDist, targetRb.linearVelocity,true);
                _Modules.DataLink.UpdateTNS_Global(targetRb);

                yield return null;
            }
        }
        finally
        {
            _Modules.DataLink.UpdateTNS_Global(null);
            _Modules.DataLink.SetSTTTargetData(0, 0, Vector2.zero, false);            
            onTerminate?.Invoke();
        }
    }
    #endregion
    #region 시퀀스: 모노레이더
    public void Mono_ACQ()//TWS살아있어서 만약 핑이 생존한놈을찍으면문제생긴다 에당초 모노는 탐색레이더없을때만 있어야함
    {
        if (GetState() == RadarState.STT_TRK)
        {
            SetState(RadarState.SRC);
            return;
        }
        if (_orderAction != null) return;
        _orderAction = StartCoroutine(MonoRadarACQSequence());
    }
    IEnumerator MonoRadarACQSequence()
    {
        var action = _Modules.Search;
        //1.핑위치 획득
        var angle = SelectedPing.GetTNS.RAngle;
        var distance = SelectedPing.GetTNS.RDistance;
        //2.핑 초기화 및 오버라이딩
        SetState(RadarState.ACQ);
        PingClear();
        //3.제어시작
        Collider2D[] rawtargets = null;
        var filteredtargets = new List<(Collider2D collider, float score)>();
        // StartCoroutine() 쓰면 브랜치 하나더생김
        // 이건 의도한 액션캐싱에 문제를 야기할수도?
        // 어짜피 저 필드로 생명관리를 하면 굳이쓸이유가없음 어디 등록하는거도아니고
        yield return action.MonoRadersys(angle, (results) => { rawtargets = results; });
        if (rawtargets != null)
        {
            foreach (var signal in rawtargets)
            {
                // 백터추출
                Vector2 dir = (Vector2)signal.transform.position - (Vector2)transform.position;
                float currentDisplayAngle = 360f - (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f + 360f) % 360f;
                float currentDistance = dir.magnitude;
                // 오차 측정(멀수록 커짐)
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(angle, currentDisplayAngle));
                float distDiff = Mathf.Abs(distance - currentDistance);
                // 기입
                filteredtargets.Add((signal, angleDiff + distDiff));
            }
        }
        // 점수순 정렬
        if (filteredtargets.Count <= 0)
        {
            _orderAction = null;
            SetState(RadarState.SRC);
            yield break;
        }
        filteredtargets.Sort((a, b) => a.score.CompareTo(b.score));

        //락온성공
        //TODO : 일단은 각도랑 거리 0으로넘기는데뭐 나중에 위치뽑아서전달해도되고
        var sig = filteredtargets[0].collider.GetComponent<Signal>();
        var ping = PingCreate(sig, 0, 0, PingState.STT_LOCK);

        STTLock_Start(sig, ping);

        _orderAction = null;
    }

    #endregion
    #region 시퀀스: 탐색/추적레이더- 일반

    public void SRC_TRK_ACQ()
    {
        if (GetState() == RadarState.STT_TRK)
        {
            SetState(RadarState.SRC);
            return;
        }
        if (_orderAction != null) return;
        _orderAction = StartCoroutine(MultiRadarRadarACQSequence());
    }
    IEnumerator MultiRadarRadarACQSequence()
    {
        var action = _Modules.Track;

        var angle = SelectedPing.GetTNS.RAngle;
        var distance = SelectedPing.GetTNS.RDistance;
        SetState(RadarState.ACQ);

        Collider2D[] rawtargets = null;
        var filteredtargets = new List<(Collider2D collider, float score)>();
        yield return StartCoroutine(action.TrackRadersys(angle, (results) => { rawtargets = results; }));
        if (rawtargets != null)
        {
            foreach (var signal in rawtargets)
            {
                // 백터추출
                Vector2 dir = (Vector2)signal.transform.position - (Vector2)transform.position;
                float currentDisplayAngle = 360f - (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f + 360f) % 360f;
                float currentDistance = dir.magnitude;
                // 오차 측정(멀수록 커짐)
                float angleDiff = Mathf.Abs(Mathf.DeltaAngle(angle, currentDisplayAngle));
                float distDiff = Mathf.Abs(distance - currentDistance);
                // 기입
                filteredtargets.Add((signal, angleDiff + distDiff));
            }
        }
        if (filteredtargets.Count <= 0)
        {
            _orderAction = null;
            SetState(RadarState.SRC);

            yield break;
        }
        filteredtargets.Sort((a, b) => a.score.CompareTo(b.score));

        var sig = filteredtargets[0].collider.GetComponent<Signal>();
        var ping = signalMap.ContainsKey(sig) ? signalMap[sig] : PingCreate(sig, 0, 0, PingState.STT_LOCK);

        ping.SetState(PingState.STT_LOCK);
        STTLock_Start(sig, ping);

        _orderAction = null;
    }

    #endregion
    #region 데링 접근허브.. 근데솔직히맘에안듬 구획을어케나누지 
    public void UpdateAllowRange(float range, float angle, float arc, RadarSystem_Type type)
    {
        if (type == RadarSystem_Type) _Modules.DataLink.SetAllowRange(range, angle, arc);
        if (type != RadarSystem_Type.MonoRadar) VaildSelectedPings();

    }
    public STT_TargetData GetTargetData() => _Modules.DataLink.GetSTTTargetData();
    public Radar_AllowRangeSet GetAllowData() => _Modules.DataLink.GetAllowRange();
    //이거 개편필요함
    public bool Session_Start(Guided projectile)
    {
        Ping ping = null;
        if (GetState() == RadarState.SRC) ping = GetSelectPing; // 하드락이 아닌상태 => 즉탐색도중에발사
        return _Modules.DataLink.Session_Start(projectile,ping);
    }

    #endregion
    #region 이벤트 관리
    Action<Ping> OnPingUIListAdded;
    Action<Ping> OnPingUIListRemoved;
    ActionPack_RadarUI _actionUi;

    public void RequestUiActions()
    {
        var Actions = UIManager_InGame.GetAction_RadarList();
        OnPingUIListAdded += Actions.OnPingUIListAdded;
        OnPingUIListRemoved += Actions.OnPingUIListRemoved;
        _actionUi = UIManager_InGame.GetAction_RadarUI();
    }
    public void ClearUiActions()
    {
        OnPingUIListAdded = null;
        OnPingUIListRemoved = null;
        _actionUi = default;
    }   
    #endregion
}
public enum DetectionType
{
    SRC,
    TRK
}
public enum RadarState
{
    SRC,
    ACQ,
    STT_TRK,

}
// TODO :  상태-전략 혼합 패턴 개선예정 하위 모듈은 코어의 전략에 행동이 지정됨
//         다만 상태변화를 인지할수있으며 그에따라 하위모듈은 행동을 지정함
public class RadarState_Ver2 
{
    public bool Power;
    public RadarSystem_Type Type;
    public RadarState_SRC_domiain SRC;
    public RadarState_TRK_domiain TRK;

}
public enum RadarState_SRC_domiain
{
    StandBy,
    SRC,
    ACQ,
    TRK,
    ShutDown,
}
public enum RadarSystem_Type
{
    MonoRadar,
    MultiRadar,
    MultiRadar_TWS,

    //개념적 존재 솔직히 안쓸지도
    MonoTrack,
    MonoTrack_TWS
}
public enum RadarState_TRK_domiain
{
    StandBy,
    TWS,
    ACQ,
    STT_TRK,
    ShutDown,
}
[Serializable]
public class RadarModules
{
    public Radar_Search Search;
    public Radar_Track Track;
    public Radar_Control Control;
    public Radar_DataLink DataLink;
}
