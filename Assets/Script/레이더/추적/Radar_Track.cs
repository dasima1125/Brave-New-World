using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Radar_Track : MonoBehaviour
{
    [SerializeField] private RadarSpec_Track spec;
    [SerializeField] private Track_Ark TrackArc;
    [SerializeField] private AddOn_TWS tws;

    readonly float WorldAngleOffset = 90f;
    private Radar_Core core;
    int _twsWay = 1;

    [Header("Monitor")]
    [SerializeField] bool TWS_Active;
    [SerializeField] float currentViewAngle;
    [SerializeField] float currentViewBeamAngle_Local; // 이거 보여주는건 다른식이여야함
    [SerializeField] float currentViewBeamAngle_World;


    public void Init(Radar_Core core)
    {
        this.core = core;
    }

    private void FixedUpdate()
    {
        ArcUpdate(core.CurrentBeamAngle);
        core.UpdateAllowRange(spec.Range, currentViewAngle, spec.BeamArc, RadarSystem_Type.MultiRadar);
        if (TWS_Active)
        {
            BeamUpdate();
            TWS_Search();
        }


    }

    void ArcUpdate(float TargetAngle)
    {
        currentViewAngle = (360f - currentViewAngle + WorldAngleOffset) % 360f;

        currentViewAngle = Mathf.MoveTowardsAngle(currentViewAngle, (450f - TargetAngle) % 360f, spec.RotationSpeed * Time.deltaTime);
        TrackArc.TrackArcImage.fillAmount = spec.BeamArc / 360f;
        TrackArc.TrackArcTransform.localRotation = Quaternion.Euler(0, 0, currentViewAngle + (spec.BeamArc / 2f));

        currentViewAngle = 360 - (currentViewAngle + 360f - WorldAngleOffset) % 360f;

    }


    void BeamUpdate()
    {
        float maxAngle = spec.BeamArc > tws.SweepBeamArc ? spec.BeamArc / 2f - (tws.SweepBeamArc / 2f) : spec.BeamArc / 2f;
        float destination = _twsWay == 1 ? maxAngle : -maxAngle;
        currentViewBeamAngle_Local = Mathf.MoveTowardsAngle(currentViewBeamAngle_Local, destination, tws.SweepSpeed * Time.fixedDeltaTime);

        if (_twsWay == 1 && currentViewBeamAngle_Local >= destination)
            _twsWay *= -1;
        else if (_twsWay == -1 && currentViewBeamAngle_Local <= destination)
            _twsWay *= -1;

        TrackArc.TrackBeamImage.fillAmount = tws.SweepBeamArc / 360f;
        TrackArc.TrackBeamTransform.localRotation = Quaternion.Euler(0, 0, currentViewBeamAngle_Local - (spec.BeamArc / 2f) + (tws.SweepBeamArc / 2f));
        currentViewBeamAngle_World = TrackArc.TrackBeamTransform.rotation.eulerAngles.z - (tws.SweepBeamArc / 2f);

    }
    void TWS_Search()
    {
        core.TRK_ClearDetection();

        float currentAngle = currentViewBeamAngle_World;
        float RandarBeamArc = tws.SweepBeamArc;
        float startAngle = currentAngle - (RandarBeamArc / 2f);
        float endAngle = currentAngle + (RandarBeamArc / 2f);

        Collider2D[] hits = AreaScan.Arc2D(transform.position, spec.Range, startAngle, endAngle, true);

        foreach (Collider2D hit in hits)
        {
            if (!hit.TryGetComponent<Signal>(out var target)) continue;
            //거리추출
            float displaydistance = Vector2.Distance(transform.position, hit.transform.position);
            //각도추출
            Vector2 direction = hit.transform.position - transform.position;
            float preciseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float displayAngle = 360f - (preciseAngle - 90f + 360f) % 360f;

            if (core.TryGetExistingPing(target, hit, DetectionType.TRK, out Ping existingPing))
            {
                if (core.TRK_WasDetectedLastFrame(hit) || core.IsHardLocked(existingPing)) continue;
                //여긴 이프가 필요없음 아마도 하드락된상대라면 컨티뉴 걸리니깐
                existingPing.SetState(PingState.SRC_TWS);
                PingTNS PNS = new(
                    angle    :displayAngle, 
                    distance :displaydistance, 
                    pos      :target.transform.position,
                    vel      :hit.attachedRigidbody.linearVelocity
                );
                
                existingPing.transform.position = PNS.Position;
                existingPing.RxUpdate(transform.position, PNS);
            }
            else core.PingCreate(target, currentViewBeamAngle_World, displaydistance);
        }
    }


    //접근부
    public IEnumerator TrackRadersys(float angle, Action<Collider2D[]> finalCallback)
    {
        yield return ACQ(angle, 10f / 2f, 3, 0.2f, finalCallback);
    }
    public void ToggleTWSMode()
    {
        TWS_Active = !TWS_Active;        
        TrackArc.TrackBeamImage.enabled = TWS_Active;

        if (!TWS_Active)
        {
            
            core.ClearTWSPings();
            //이부분이 지터링 문제를 야기함
            //TrackArc.TrackBeamTransform.localRotation = Quaternion.identity;
            //currentViewBeamAngle_Local = 0;
            //_twsWay = 1;
        }
    }
    IEnumerator ACQ(float pointAngle, float ArcWidth, int sectorCount, float Duration, Action<Collider2D[]> result)
    {
        float anglePerSector = ArcWidth * 2f / sectorCount;   // 각 섹터당 각도 크기 => (아크 절대값 * 2) / 섹터 수
        float startAngle = 450f - pointAngle - ArcWidth;

        HashSet<Collider2D> storage = new();

        for (int i = 0; i < sectorCount; i++)
        {
            float sectorStart = NormalizeAngle(startAngle + anglePerSector * i);
            float sectorEnd = NormalizeAngle(startAngle + anglePerSector * (i + 1));

            Collider2D[] hits = AreaScan.Arc2D(transform.position, 150f, sectorStart, sectorEnd, true);
            storage.UnionWith(hits);
            yield return new WaitForSeconds(Duration / sectorCount);
        }
        result?.Invoke(storage.ToArray());
        yield return null;
    }
    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }

}
[Serializable]
public class Track_Ark
{
    public Transform TrackArcTransform;
    public Image TrackArcImage;
    public Transform TrackBeamTransform;
    public Image TrackBeamImage;


}

[Serializable]
public class RadarSpec_Track
{
    public float Range;        // 탐지 거리
    [SerializeField, Range(0, 360)]
    public float BeamArc;      // 빔 각
    public float RotationSpeed; // 회전 속도
}
[Serializable]
public class AddOn_TWS
{
    public float SweepBeamArc;     // 빔범위  
    public float SweepSpeed;   // 빔속도
}


