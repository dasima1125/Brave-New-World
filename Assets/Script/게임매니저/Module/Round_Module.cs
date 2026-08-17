using System;
using System.Collections.Generic;
using UnityEngine;

public class Round_Module : MonoBehaviour
{
    GameManager_Scene core;
    [SerializeField] RoundDataSO[] RoundDatas;  //아마 일주일만 할듯    
    [SerializeField] RoundDataSO NowRoundData;
    [SerializeField] Round_Recode RoundRecodes; //라운드 진행기록  사실 가리는게맞는데 일단 디버깅으로 표시해야함           
    [SerializeField] int RoundIndex = 0;
    public void Init(GameManager_Scene gameManager)
    {
        core = gameManager;
        
        G_Excutor.Subscribe("GameDefeat", RoundKill_LOSE);
        
        GameStart();
        //TODO : 이건 이니트보단 게임 시작시 별도의 메서드로 처리해야함 
        //       로드한게임일수도 새 게임일수도있으니.. 일단은 임시로 만듬 테스트용이니 처음부터
    }
    public void GameStart() => LoadRound(0);
    public void GameStart(int road) => LoadRound(road);

    // 라운드 로더
    void LoadRound(int index)
    {
        if (index >= RoundDatas.Length)
        {
            Debug.Log("<color=cyan>[LOAD] 모든 라운드 완료</color>");
            // 나중에 런 클리어 처리
            return;
        }

        RoundIndex = index;
        NowRoundData = Instantiate(RoundDatas[index]); // so원본이아니라 사본을받아와야함
                                                       // 순서정렬도 추가해야할듯?
        Debug.Log($"<color=cyan>[LOAD] Day {RoundIndex + 1} 로드</color>");
    }
    // 진행자
    void Call_StartRound()
    {
        if (core.GetPhase() == Phase_Main.READY)
        {
            RoundRecodes = new();
            if (NowRoundData == null)
            {
                Debug.Log("<color=red>[ERROR] 데이터 로드가 안 되었거나 모든 라운드를 클리어함</color>");
                return;
            }
            core.SetPhase(Phase_Main.PLAY);
            core.Request_StartRound(NowRoundData.RoundTime, Call_EndRound, Call_ReceiveDuration);
        }
        else Debug.Log("레디상태가 아님");
    }
    void Call_EndRound()
    {
        core.SetPhase(Phase_Main.END);
        Debug.Log($"<color=yellow>[Round] 라운드 종료사유 : {RoundRecodes.Outcome}</color>");
        IsVictory(); // <- 이건 사실쓰면안됨 애당초 라운드를통으로넘김 
                     // 아직 유아이개념을 정립못해서임
        UIManager_InGame.Test_RoundResult(RoundRecodes.Outcome);
    }

    void Call_ReceiveDuration(float deltaTime) // 이게 액션팩 작동자임 와일조건맞으면 실행 근데구조 개떡같음..
                                               // 아마 여기서 뭐 인보크등 뭐든해가지고 잘 만지면될려나?
    {
        RoundRecodes.Update_Time(NowRoundData.RoundTime, deltaTime);
        foreach (ActionPack_Test actionPack in NowRoundData.testActionPack.Actions)
        {
            while (actionPack.bookActionIndex < actionPack.BookActionTime.Count &&
                RoundRecodes.ElapsedTime >= actionPack.BookActionTime[actionPack.bookActionIndex])
            {
                actionPack.Action();
                actionPack.bookActionIndex++;
            }
        }
    }
    bool IsVictory()
    {
        if (RoundRecodes.Outcome == Round_Outcome.PASS) OnVictory();
        else OnDefeat();

        return true;
    }

    void OnVictory()
    {
        Debug.Log($"<color=green>[Round] Day {RoundIndex + 1} 승리</color>");
        // 일단은 승리시에 옴기지만 이건 사실 승리시가 아니라 승리후 승리패널에서 다음날 이벤트를써야
        // 다음날로 가는게맞음
        // 근데 그걸 만들어야넣지..

        RoundIndex++;
        LoadRound(RoundIndex);
        core.SetPhase(Phase_Main.READY);
    }
    void OnDefeat()
    {
        Debug.Log($"<color=red>[Round] Day {RoundIndex + 1} 패배</color>");
    }
    // 테스트용
    public void RoundStart() => Call_StartRound();
    public void RoundKiil_DEBUG()
    {
        RoundRecodes.Outcome = Round_Outcome.DEBUG;
        core.SetPhase(Phase_Main.END);
    }
    public void RoundKill_LOSE()
    {
        RoundRecodes.Outcome = Round_Outcome.LOSE;
        core.SetPhase(Phase_Main.END);
    }


}

[Serializable]
public class Round_Recode
{
    public Round_Outcome Outcome; // 종료 사유
    public float ElapsedTime; // 진행시간
    public float AchievementPercent_Time; // 진행도(시간)
    public int Score; // 점수
    public void Update_Time(float RoundTime, float Duration)
    {
        if (Outcome != Round_Outcome.PASS) return; //PASS가 아닐때 = 게임종료시점. 더이상 갱신할필요가 없음

        float TotalTime = RoundTime; // 임시
        ElapsedTime = TotalTime - Duration;
        AchievementPercent_Time = ElapsedTime / TotalTime * 100f;
    }
}
public enum Round_Outcome
{
    PASS,
    DEBUG,
    LOSE,
}

[Serializable]
public class ActionPack_Sequence
{
    public List<ActionPack_Test> Actions = new();
}
[Serializable]
public class ActionPack_Test
{
    public RoundActionSO ActionSO;
    public int bookActionIndex = 0;
    public List<float> BookActionTime = new();
    public void Action() => ActionSO.Action();  
}