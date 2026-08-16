using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LinePath : MonoBehaviour
{
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    public int PointCount => lineRenderer.positionCount;

    public Vector3 GetPoint(int index)
    {
        if (index >= 0 && index < lineRenderer.positionCount)
        {
            return lineRenderer.GetPosition(index);
        }
        
        Debug.LogWarning($" 인덱스 {index}오버.");
        return Vector3.zero;
    }
}