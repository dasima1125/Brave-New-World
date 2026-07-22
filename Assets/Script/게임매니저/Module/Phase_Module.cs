using System;
using System.Collections;
using UnityEngine;

public class Phase_Module : MonoBehaviour
{
    GameManager_Scene core;
    // 메인페이즈
    [field: SerializeField]public Phase_Main CurrentPhase { get; private set; }
    [field: SerializeField]public Progression CurrentProgress  { get; private set; }
    public void Init(GameManager_Scene gameManager) => core = gameManager;
   
    Coroutine PlayRoutine;

    private void Start() // 필요없을지도 .. 이니트가 해야할거같은데?
    {
        SetPhase(Phase_Main.READY);
    }
    // 나중에 지우셈 이건  테스트용임
    public void BtnClick_Ready() => RunPhase_Ready();
    public void BtnClick_Play() => RunPhase_Play();
    public void BtnClick_End() => RunPhase_End();
    public void BtnClick_Pause() => RunPhase_Pause();
    public void BtnClick_StartRound() => Run_Routine(5f ,null, null);

    // 외부 조정
    public bool RunPhase_Ready() // 라운드 준비상태
    {
        if (CurrentPhase == Phase_Main.READY) return false;

        SetPhase(Phase_Main.READY);
        return true;
    }
    public bool RunPhase_Play() // 라운드 진행상태
    {
        if (CurrentPhase == Phase_Main.PLAY) return false;

        SetPhase(Phase_Main.PLAY);
        return true;
    }
    public bool RunPhase_End() // 라운드 종료 상태 (성공/실패 여부와 상관없이 해당 라운드의 플레이가 끝난 후 대기)
    {
        if (CurrentPhase == Phase_Main.END) return false;

        SetPhase(Phase_Main.END);
        return true;
    }
    public void RunPhase_Pause() // 라운드 일시정지상태(메인 페이즈가아닌 프로그래스 페이즈 제어)
    {
        if (CurrentProgress == Progression.RUN)
        {
            SetProgress(Progression.STOP);
            Debug.Log("<color=yellow>[Pause] 일시정지 ON</color>");
        }
        else if (CurrentProgress == Progression.STOP)
        {
            SetProgress(Progression.RUN);
            Debug.Log("<color=green>[Pause] 일시정지 OFF</color>");
        }
    }
    public bool Run_Routine(float GameTime, Action onEnd, Action<float> onProgress) 
    {
        if (PlayRoutine != null)
        {
            Debug.Log("<color=red>[중복] 루틴 실행중</color>");
            return false;
        }
        PlayRoutine = StartCoroutine(PlayPhaseRoutine(GameTime, onEnd, onProgress));
        return true;
    }
    public void Kill_Routine()
    {
        if (PlayRoutine != null) StopCoroutine(PlayRoutine);
    }
    // 내부 접근
    void SetPhase(Phase_Main phase)
    {
        CurrentPhase = phase;
        switch (CurrentPhase)
        {
            case Phase_Main.READY:
                Debug.Log("<color=yellow>[Ready] 상태</color>");
                break;

            case Phase_Main.PLAY:
                Debug.Log($"<color=green>[Play] 상태</color>");
                break;

            case Phase_Main.END:
                Debug.Log("<color=red>[End] 상태</color>");
                break;
        }
    }
    void SetProgress(Progression progress) => CurrentProgress = progress;

    private IEnumerator PlayPhaseRoutine(float GameTime, Action onEnd, Action<float> onProgress)
    {
        float CurrentTime = GameTime;
        try
        {
            Debug.Log($"<color=green>[Routine] 루틴 : 시작");
            while (CurrentTime > 0 && CurrentPhase == Phase_Main.PLAY)
            {
                if (CurrentProgress == Progression.STOP)
                {
                    yield return null;
                    continue;
                }
                CurrentTime -= G_Time.Dtime;
                onProgress?.Invoke(Mathf.Max(0, CurrentTime));

                yield return null;
            }
        }
        finally
        {
            Debug.Log($"<color=green>[Routine] 루틴 : 종료");
            PlayRoutine = null;
            onEnd?.Invoke();
        }
    }
}
public enum Phase_Main { READY, PLAY, END }
public enum Progression { RUN, STOP }
