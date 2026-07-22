using System.Collections.Generic;
using UnityEngine;
public class DotPath_Baker : MonoBehaviour
{
    [Header("경로 SO 슬롯")]
    public DotPath PathSO;
    public List<Transform> waypoints = new();
    public bool showDebugPath = true;

    public void BakePath()
    {
        // 시퀸스 요약
        // 1 so 초기화
        // 2 웨이포인트 순회
        // 3 경로 추가(vector)
        // 4 세이브
        if (PathSO == null) return;

        PathSO.points.Clear();

        foreach (Transform t in waypoints)
        {
            if(t != null) PathSO.points.Add(t.position);
        }

        SaveAsset();
    }
    public void AppendPath()
    {
        if (PathSO == null) return;

        foreach (Transform t in waypoints)
        {
            if(t != null) PathSO.points.Add(t.position); 
        }

        SaveAsset();

    }
    public void ClearPath()
    {
        if (PathSO == null) return;
        PathSO.points.Clear();

        SaveAsset();

    }
    void SaveAsset()
    {
        #if UNITY_EDITOR // 메모리상 저장이면 뭐쓸필요없겠지 하지만 우린 보조기억장치에 저장해야겠지?
        UnityEditor.EditorUtility.SetDirty(PathSO);
        UnityEditor.AssetDatabase.SaveAssets();
        #endif
    }

    
    void OnDrawGizmos()
    {

        if (!showDebugPath || PathSO == null || PathSO.points.Count < 1) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < PathSO.points.Count; i++)
        {
            Gizmos.DrawSphere(PathSO.points[i], 0.15f);
            if(i < PathSO.points.Count - 1) Gizmos.DrawLine(PathSO.points[i], PathSO.points[i + 1]);
        }
    }

}
