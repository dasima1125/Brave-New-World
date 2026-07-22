using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DotPath_Baker))]
public class DotPath_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DotPath_Baker baker = (DotPath_Baker)target;
        GUILayout.Space(15);

        GUILayout.BeginHorizontal();
        {
            GUI.backgroundColor = new Color(0.3f, 0.8f, 0.4f); 
            if (GUILayout.Button("Create Path", GUILayout.Height(35)))
            {
                if (baker.PathSO != null && baker.PathSO.points.Count > 0)
                {
                    if (EditorUtility.DisplayDialog("경고: 데이터 소멸 가능", "기존 SO 데이터를 지우고 리스트의 웨이포인트로 새로 구우시겠습니까?", "예", "아니오"))
                    {
                        baker.BakePath();
                    }
                }
                else baker.BakePath();
            }

            GUI.backgroundColor = new Color(0.2f, 0.6f, 1f); 
            if (GUILayout.Button("Append Path", GUILayout.Height(35)))
            {
                baker.AppendPath();
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(15);

        DrawThinLine(new Color(0.3f, 0.3f, 0.3f, 0.5f), 1, 10);

    
        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f); 
        if (GUILayout.Button("Clear Path", GUILayout.Height(35)))
        {
            if (EditorUtility.DisplayDialog("경고: 데이터 소멸 가능", "SO의 경로 데이터를 완전히 비우시겠습니까?", "예", "아니오"))
            {
                baker.ClearPath();
            }
        }
        
        GUI.backgroundColor = Color.white;
    }


    private void DrawThinLine(Color color, int thickness, int space)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, thickness);
        rect.height = thickness;
        EditorGUI.DrawRect(rect, color);
        GUILayout.Space(space);
    }
}