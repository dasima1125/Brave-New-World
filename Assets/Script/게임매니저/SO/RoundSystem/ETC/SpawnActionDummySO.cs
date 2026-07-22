using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New SpawnDummy", menuName = "Round/RoundAction/SpawnDummy")]
public class SpawnActionDummySO : RoundActionSO
{
    public SpawnDTO_Basic DTO;
    public override void Action() => G_Excutor.Call(RequestKey, DTO);
}

