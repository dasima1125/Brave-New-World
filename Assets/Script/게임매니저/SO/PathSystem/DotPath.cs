using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDotPath", menuName = "Path/Dot_Path")]
public class DotPath : ScriptableObject
{
    // 몹들이 참조할 경량화된 순수 좌표 리스트
    public List<Vector3> points = new();
}