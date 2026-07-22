using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Radar_Search : MonoBehaviour
{
    [SerializeField] Transform SweepBarTransForm;
    [SerializeField] Transform SweepBarShadowTransForm;

    [SerializeField] private RadarSpec_Search spec;
    public bool debug;
    private Radar_Core core;
    [Header("Monitor")]
    [SerializeField] float currentViewAngle;

    public void Init(Radar_Core core)
    {
        this.core = core;
    }

    void FixedUpdate()
    {
        RadarState currentState = core.GetState();

        bool isSearching = currentState == RadarState.SRC;
        if (SweepBarShadowTransForm.gameObject.activeSelf != isSearching && core.RadarSystem_Type == RadarSystem_Type.MonoRadar)
            SweepBarShadowTransForm.gameObject.SetActive(isSearching);
        

        if(core.RadarSystem_Type== RadarSystem_Type.MonoRadar)//바보같은방식
        switch (currentState)
        {
            
            case RadarState.SRC:
                RotateBar(Time.deltaTime * spec.RotationSpeed);
                Search();
                SweepBarShadowTransForm.gameObject.SetActive(true);
                break;
    

            case RadarState.STT_TRK:
                RotateBar_OverRide();
                break;
        }
        else // 일단 나도이게 더럽다는건 알고는있음
        {
            RotateBar(Time.deltaTime * spec.RotationSpeed);
            Search();
        }
        float corrected = 360f - (SweepBarTransForm.eulerAngles.z - 90f);
        currentViewAngle = (corrected + 360f) % 360f;

        core.UpdateAllowRange(spec.Range, currentViewAngle, spec.BeamArc,RadarSystem_Type.MonoRadar);
    }

    void Search()
    {
        core.SRC_ClearDetection();

        float currentAngle = SweepBarTransForm.eulerAngles.z;
        float RandarBeamArc = 2f;
        float startAngle = currentAngle - (RandarBeamArc / 2f);
        float endAngle = currentAngle + (RandarBeamArc / 2f);

        Collider2D[] hits = AreaScan.Arc2D(transform.position, spec.Range, startAngle, endAngle, debug);

        foreach (Collider2D hit in hits)
        {
            if (!hit.TryGetComponent<Signal>(out var target)) continue;
            float displayAngle = 360f - (currentAngle - 90f + 360f) % 360f;
            float displaydistance = Vector2.Distance(transform.position, hit.transform.position);

            if (core.TryGetExistingPing(target, hit, DetectionType.SRC, out Ping existingPing))
            {
                if (core.SRC_WasDetectedLastFrame(hit)) continue;
                PingTNS PNS = new(
                    angle    :displayAngle, 
                    distance :displaydistance, 
                    pos      :target.transform.position
                );
                if(existingPing.GetState != PingState.SRC_Nomarl) continue;
                
                existingPing.transform.position = PNS.Position;
                existingPing.RxUpdate(transform.position,PNS);
                
            }
            else core.PingCreate(target, displayAngle, displaydistance);
        }
        core.SRC_SwapDetectionFrames();
    }
    void RotateBar(float deltaAngle) => SweepBarTransForm.eulerAngles -= new Vector3(0, 0, deltaAngle);
    void RotateBar_OverRide()
    {
        float targetAngle = core.GetTargetData().TargetAngle;

        // 유니티 좌표계에 맞게 변환 (기존 MonoRadarChaser에 있던 로직)
        float convertAngle = Mathf.Repeat(450f - targetAngle, 360f);

        float next = Mathf.MoveTowardsAngle(
            SweepBarTransForm.eulerAngles.z,
            convertAngle,
            spec.RotationSpeed * Time.deltaTime // 추적 속도
        );

        SweepBarTransForm.eulerAngles = new Vector3(0, 0, next);
    }
    //접근부
    public IEnumerator MonoRadersys(float angle, Action<Collider2D[]> finalCallback)
    {
        float ConvertAngle = Mathf.Repeat(450f - angle, 360f);
        while (Mathf.Abs(Mathf.DeltaAngle(SweepBarTransForm.eulerAngles.z, ConvertAngle)) > 0.1f)
        {
            float next = Mathf.MoveTowardsAngle(
                SweepBarTransForm.eulerAngles.z,
                ConvertAngle,
                spec.RotationSpeed * Time.deltaTime // 추적 속도
            );
            SweepBarTransForm.eulerAngles = new Vector3(0, 0, next);
            yield return null;
        }
        yield return ACQ(angle, 10f / 2f, 3, 0.2f, (callback) =>
        {
            //Collider2D[] hits = callback;
            finalCallback?.Invoke(callback);
        });
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
public class RadarSpec_Search
{
    public float Range;        // 탐지 거리
    public float BeamArc;      // 빔 각도
    public float RotationSpeed; // 회전 속도
}
