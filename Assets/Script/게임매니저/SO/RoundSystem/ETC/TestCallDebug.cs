using UnityEngine;

[CreateAssetMenu(fileName = "New TestCallDebug", menuName = "Round/RoundAction/TestCallDebug")]
public class TestCallDebug : RoundActionSO, IAction_Receiver // 오브젝트 바인더의 개념 이게없으면 추적을 할수가없음 
{
    public override void Action() => G_Excutor.Call(RequestKey);
    public void Action(GameObject instance) => G_Excutor.Call(RequestKey, instance);
}
