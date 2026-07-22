using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoundData", menuName = "Round/RoundData")]

public class RoundDataSO : ScriptableObject
{
    public string RoundScriptName; // 테스트용
    public float RoundTime;
    public ActionPack_Sequence testActionPack; // 테스트용
}
