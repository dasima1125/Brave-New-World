using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Ping : MonoBehaviour
{
    //private Radar_Core _radar;
    private long _pid; //핑 고유아이디
    private Vector3 _broadcastPos;
    public PingTNS _PingTNS; // << 프리베이트로 해야함 일단 확인용으로 열어둔거
    private bool PingSelect;
    private float disappearTimer;
    private float disappearTimerMax;

    // 핑의 상대각, 상대거리 ,위치, 진행방향 ,가속(진행방향이랑 가속은 밸로시티로 한번에받아야하나?)

    [SerializeField] PingState _currentState;
    [SerializeField] PingStateGroup[] pingStateGroups;
    Dictionary<PingState, GameObject> _stateMap = new();
    SpriteRenderer[] PingUIs;
    //선택 UI관련 - 상태머신이 아닌 다른개념
    [SerializeField] Transform VelocityPointer;
    [SerializeField] GameObject SelectedUI;
    SpriteRenderer[] SelectedRects;
    Action OnDestroyAction;

   

    public void Init(long PID,Vector3 broadcastPos, PingTNS PingTNS, float duration)
    {
        foreach (var group in pingStateGroups) _stateMap[group.state] = group.rootGroup;
        
        disappearTimerMax = duration;
        disappearTimer = disappearTimerMax;

        _pid = PID;
        _PingTNS = PingTNS;
        _broadcastPos = broadcastPos;
        _currentState = PingState.SRC_Nomarl;
        ApplyStateVisuals();
    }

    /// <summary>
    /// 주의 : 핑의 위치재지정은 반드시 핑이아닌 호출자가 먼저 지정하주고 위치를 주입할것
    /// </summary>
    public void RxUpdate(Vector3 broadcastPos, PingTNS PingTNS)
    {
        _PingTNS = PingTNS;
        _broadcastPos = broadcastPos;
        disappearTimer = disappearTimerMax;
    }
    

    void FixedUpdate()
    {
        PingLife();
        FadeOut();
    }

    void PingLife()
    {
        disappearTimer -= Time.deltaTime;
        if (_currentState == PingState.SRC_TWS)
        {
            transform.position += _PingTNS.velocity * Time.fixedDeltaTime;

            Vector2 dir = transform.position - _broadcastPos;
            _PingTNS.RDistance = dir.magnitude;
            _PingTNS.RAngle = 360f - (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f + 360f) % 360f;

            UpdateVelocityUI(_PingTNS.velocity);
        }
        if(_currentState == PingState.STT_LOCK) UpdateVelocityUI(_PingTNS.velocity);
        if (disappearTimer <= 0) KillPing();
    }
    void FadeOut()
    {
        if (PingUIs == null) return;
        float threshold = 3f;
        float alpha = disappearTimer >= threshold ? 1f : Mathf.Clamp01(disappearTimer / threshold);
        if (PingSelect)
        {
            foreach (var rect in SelectedRects)
            {
                Color colorS = rect.color;
                colorS.a = alpha;
                rect.color = colorS;
            }
        }
        foreach (var UI in PingUIs)
        {
            Color colorP = UI.color;
            colorP.a = alpha;
            UI.color = colorP;
        }

    }
    void ApplyStateVisuals()
    {
        foreach (var obj in _stateMap.Values) obj.SetActive(false);
        switch (_currentState)
        {
            case PingState.SRC_Nomarl:
                if (_stateMap.TryGetValue(PingState.SRC_Nomarl, out GameObject normalRoot))
                {
                    normalRoot.SetActive(true);
                    PingUIs = normalRoot.GetComponentsInChildren<SpriteRenderer>(true);
                    VelocityPointer.gameObject.SetActive(false);
                }
                break;
            case PingState.SRC_TWS:
                if (_stateMap.TryGetValue(PingState.STT_LOCK, out GameObject twsRoot))
                {
                    twsRoot.SetActive(true);
                    PingUIs = twsRoot.GetComponentsInChildren<SpriteRenderer>(true);
                }
                break;
            case PingState.STT_LOCK:
                if (_stateMap.TryGetValue(PingState.STT_LOCK, out GameObject lockRoot))
                {
                    lockRoot.SetActive(true);
                    PingUIs = lockRoot.GetComponentsInChildren<SpriteRenderer>(true);
                }
                break;
        }
    }
    void UpdateVelocityUI(Vector2 velocity)
    {
        if (velocity.sqrMagnitude < 0.1f)
        {
            VelocityPointer.gameObject.SetActive(false);
            return;
        }
        if (!VelocityPointer.gameObject.activeSelf) VelocityPointer.gameObject.SetActive(true);
        VelocityPointer.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg - 90f);

    }
    /// 접근부
    
    public void SetState(PingState newState)
    {
        if (_currentState == newState) return;
        _currentState = newState;
        ApplyStateVisuals();
    }
    public long GetPID => _pid;
    public bool GetSelect => PingSelect;
    public PingState GetState  => _currentState;
    public PingTNS GetTNS      => _PingTNS ;
    //public Vector3 GetVelocity => _PingTNS.velocity;
    public void SetSelect(bool type)
    {
        SelectedUI.SetActive(type);
        SelectedRects ??= SelectedUI.GetComponentsInChildren<SpriteRenderer>(true);
        PingSelect = type;
    }
    public void KillPing()
    {
        OnDestroyAction?.Invoke();
        OnDestroyAction = null;
        Destroy(gameObject);
    }
    public void ActionInjector(Action OnDestroyAction) => this.OnDestroyAction = OnDestroyAction;
}

public enum PingState
{
    SRC_Nomarl,
    SRC_TWS,
    STT_LOCK,
    ARM
}
[Serializable]
public struct PingTNS
{
    public bool IsValid;
    public float RAngle;
    public float RDistance;
    public float RAltitude;//이건아직안씀
    public Vector3 Position;
    public Vector3 velocity;    
    public PingTNS(float angle, float distance, Vector3 pos, Vector3 vel = default, float alt = default)
    {
        this.IsValid = true; 
        this.RAngle = angle;
        this.RDistance = distance;
        this.Position = pos;
        this.velocity = vel;
        this.RAltitude = alt;
    }
}
[Serializable]
public class PingStateGroup
{
    public PingState state;
    public GameObject rootGroup;
}