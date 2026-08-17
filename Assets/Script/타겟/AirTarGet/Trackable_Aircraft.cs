using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Trackable_Aircraft : TrackableObject, ISelectable, IDamageable
{
    //이건 꼭 필요한건진 모르겠음

    public bool IsDestroyed => throw new NotImplementedException();
    public void TakeDamage(float damage) => Debug.Log("타격받음");


    [SerializeField] TargetDataSO data;
    [SerializeField] float globalProgress;
    [SerializeField] float segmentProgress;
    DotPathVer2 WayMap;
    int WayIndex;
    // 다만 관건은 음.. 플로트는 이놈이가지는게맞겠지? 아마도말이지?
    // 셀렉트 인터페이스를이용하면될꺼야 아마?
    
    
    bool _active = false;
    protected override void InitChain()
    {
        base.InitChain();

        currentTimer = 0f;

    }
    public void SetPath(DotPathVer2 path)
    {
        WayMap = path;
        WayIndex = 1;

        _active = true;
    }




    //테스트용 일단 이니트를 할때 반드시 아마 딕셔너리든 어디든 넣는걸 기점으로해야할듯  물론 이놈이 아닌 
    //오브젝트쪽으로
    float currentTimer;
    float LifeTime = 40;
    void Update()
    {
        if (!ActiveObject || !_active) return;

        currentTimer += G_Time.Dtime;
        AltPath();
        WayTracker_TestAirCraft();
        if (currentTimer >= LifeTime) Kill();
    }
    void WayTracker_TestAirCraft()
    {
        if (WayMap == null || WayMap.Waypoints.Count == 0 || WayIndex >= WayMap.Waypoints.Count)
            return;// 정지시킬려면 백터 제로를 걸던가

        Vector2 WayPos = WayMap.Waypoints[WayIndex];
        Vector2 MyPos = transform.position;

        // 현재 위치에서 목적지를 향하는 순수 방향 벡터 추출
        Vector2 direction = (WayPos - MyPos).normalized;
        rb.linearVelocity = data.speed * G_Time.GetScale_W() * direction;

        float distanceSqr = (WayPos - MyPos).sqrMagnitude;
        float arrivalThreshold = 0.04f; // 거리 0.2f 미만일 때 도달 판정 (0.2 * 0.2)
        CalcPathProgress(WayPos, MyPos);

        if (distanceSqr < arrivalThreshold)
        {
            WayIndex++;

            // 나중에 환형(Loop) 제어를 넣거나 풀링 반환 처리를 할 분기 구역
            // 예: if (WayIndex >= WayMap.points.Count && isLoop) WayIndex = 0;
            // 근데일단 패스먼저 패스끝나고 이제 라운드so슬슬 구조화해야함

            //테스트용이니깐 환형처리하자 계속돌게하고 수명은 뭐 무한걸자 아닌가 ? 수명 20초는?

        }

    }
    void CalcPathProgress(Vector2 targetPos, Vector2 currentPos)
    {
        if (WayIndex == 0) return;

        Vector2 prevPos = WayMap.Waypoints[WayIndex - 1];
        float segmentLength = Vector2.Distance(prevPos, targetPos);

        float segRatio = 0f;
        if (segmentLength > 0f)
        {
            float currentLength = Vector2.Distance(prevPos, currentPos);
            segRatio = Mathf.Clamp01(currentLength / segmentLength);
        }

        float totalSegments = WayMap.Waypoints.Count - 1;
        float globalRatio = Mathf.Clamp01((WayIndex - 1 + segRatio) / totalSegments);

        segmentProgress = segRatio * 100f;
        globalProgress = globalRatio * 100f;
    }
    void AltPath()
    {
        if (data == null || data.flightCurve == null) return;
        float progressNormalized = globalProgress / 100f;
        float heightRatio = data.flightCurve.Evaluate(progressNormalized);

        altitude = data.altitude * heightRatio;
    }

    protected override void Kill()
    {
        _active = false;
        Debug.Log("[철수] 비행체");
        base.Kill();

    }
}
