using System;
using UnityEngine;

//앤 수명자체가 의미없을듯 뭐 도중에재밍같은걸로경로를꺽고 다시 돌아오지않는이상 .. 근데그기능이필요할거같진않음
public class Trackable_Missile : TrackableObject, ISelectable, IDamageable
{

    public bool IsDestroyed => throw new System.NotImplementedException();//암만봐도 이건 하자마자 킬나올꺼라
                                                                          //문제가없음
                                                                          //근데연출과함게 한다면 또몰라
                                                                          //또근데 레이더가어케 그거하나하나 알아야해? 그것도말이안되잔아
                                                                          //흠

    [SerializeField] TargetDataSO data;
    DotPathVer2 WayMap;
    [SerializeField] float globalProgress;
    [SerializeField] float segmentProgress;
    [SerializeField] int WayIndex;






    bool _active = false;
    protected override void InitChain()
    {
        base.InitChain();
    }
    public void SetPath(DotPathVer2 path)
    {
        segmentProgress = 0f;
        globalProgress = 0f;

        WayMap = path;
        WayIndex = 1;

        AltPath();
        _active = true;

    }
    void FixedUpdate()
    {
        if (!ActiveObject || !_active) return;
        WayTracker_AG();
        AltPath();

    }
    void WayTracker_AG()
    {
        if (WayMap == null || WayMap.Waypoints.Count == 0 || WayIndex >= WayMap.Waypoints.Count)
        {
            //예외성 종료
            Kill();
            return;
        }

        Vector2 WayPos = WayMap.Waypoints[WayIndex];
        Vector2 MyPos = transform.position;


        // 방향추출
        Vector2 direction = (WayPos - MyPos).normalized;
        rb.linearVelocity = data.speed * G_Time.GetScale_W() * direction;

        float distanceSqr = (WayPos - MyPos).sqrMagnitude;
        float arrivalThreshold = 0.04f;

        CalcPathProgress(WayPos, MyPos);//진행상태 업데이트
        //TODO : 이동량 기반으로 판정요함
        if (distanceSqr < arrivalThreshold)
        {
            if (WayIndex == WayMap.Waypoints.Count - 1)
            {
                //행동로직
                Impact();
                return;
            }
            WayIndex++;
        }
    }
    void AltPath()
    {
        if (data == null || data.flightCurve == null) return;
        float progressNormalized = globalProgress / 100f;
        float heightRatio = data.flightCurve.Evaluate(progressNormalized);

        altitude = data.altitude * heightRatio;
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


    void Impact()
    {
        rb.linearVelocity = Vector2.zero;
        segmentProgress = 100f;
        globalProgress = 100f;
        AltPath();
        Debug.Log("[미사일] 폭파실행");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable_Ally>(out var target))
            {
                target.TakeDamage(1);
            }
        }
        Kill();
    }
    public int testhp = 1;

    public void TakeDamage(float damage)
    {
        testhp -= (int)damage;
        if (testhp <= 0)
        {
            Debug.Log("[미사일] 요격됨");
            Kill();
        }
    }



    protected override void Kill()
    {
        _active = false;
        base.Kill();

    }
}
