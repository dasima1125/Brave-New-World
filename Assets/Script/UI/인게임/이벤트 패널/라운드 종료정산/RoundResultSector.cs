using UnityEngine;

public class RoundResultSector : MonoBehaviour
{
    UIManager_InGame _core;
    [SerializeField] RoundResultPanel_AddOn panelWin;
    [SerializeField] RoundResultPanel_AddOn panelLose;
    public void Init(UIManager_InGame core) => _core = core;
    public void Open(Round_Outcome outcome, int data)
    {
        switch (outcome)
        {
            case Round_Outcome.PASS:
                Debug.Log($"<color=green>[Round] 라운드 승리</color>");
                panelWin.gameObject.SetActive(true);
                break;
            case Round_Outcome.LOSE:
                Debug.Log($"<color=red>[Round] 라운드 패배</color>");
                panelLose.gameObject.SetActive(true);
                break;
            case Round_Outcome.DEBUG:
                Debug.Log($"<color=yellow>[Round] 라운드 디버그</color>");
                break;
        }
    }
    public void Close()
    {
        panelWin.gameObject.SetActive(false);
        panelLose.gameObject.SetActive(false);

        //철수명령 섹터를 스스로 철수시키되 섹터한테 철수하라가아님.. 아니면이벤트를쓰던가 
        //그래도될려나? 근데 그러면 열린지아닌지는? 아 어짜피 이벤트지?
    }
    /*
    */


}
