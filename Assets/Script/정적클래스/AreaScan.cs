using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AreaScan
{
    public static Collider2D[] Arc2D(Vector2 point, float radius, float startAngle, float endAngle, bool drawDebug = false)
    {
        float Duration = 0;
        List<Collider2D> output = new();
        if (drawDebug)
        {
            Vector3 sDir = new(Mathf.Cos(startAngle * Mathf.Deg2Rad), Mathf.Sin(startAngle * Mathf.Deg2Rad), 0);
            Vector3 eDir = new(Mathf.Cos(endAngle * Mathf.Deg2Rad), Mathf.Sin(endAngle * Mathf.Deg2Rad), 0);
            
            Debug.DrawLine(point, point + (Vector2)sDir * radius, Color.green, Duration);
            Debug.DrawLine(point, point + (Vector2)eDir * radius, Color.green, Duration);
            Debug.DrawLine(point + (Vector2)sDir * radius, point + (Vector2)eDir * radius, Color.green, Duration);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(point, radius);
        foreach (Collider2D hit in hits)
        {
            float angle = GetAnglePoint(point, hit);
            if (IsAngleBetween(angle, startAngle, endAngle))
            {
                output.Add(hit);
            }
        }
        return output.ToArray();
    }

    private static float GetAnglePoint(Vector2 point, Collider2D hit)
    {
        Vector2 direction = (Vector2)hit.transform.position - point;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        return angle;
    }
    //나중에 처리 : TODO 이론상 .. 원형 콜라이더라면 중심점을 잡되 반지름만큼 여유분을 준다면?
    // 실제 아크는각 계산을 할때  IsAngleBetween 여기 넘기기직전 콜라이더의 반지름을  보정해 넘기는방법을 써봐도 될려나?
    // 돌겠네 진짜.. 걸침문제를 해결해야하는데.. 일단은냅두자.
    private static bool IsAngleBetween_Low(float angle, float start, float end)
    {
        if (start == end) return false;

        if (start < end)
            return angle >= start && angle <= end;
        else
            return angle >= start || angle <= end;
    }
    private static bool IsAngleBetween(float angle, float start, float end)
    {
        angle = NormalizeAngle(angle);
        start = NormalizeAngle(start);
        end = NormalizeAngle(end);

        if (start == end)
        {
            Debug.LogWarning("Ignore Range");
            return false;
        }    

        if (start < end)
            return angle >= start && angle <= end;
        else
            return angle >= start || angle <= end;
    }
    private static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }

    
}
