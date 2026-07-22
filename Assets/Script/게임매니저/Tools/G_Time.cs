using UnityEngine;
public class G_Time : G_Tools<G_Time>
{
    private float RawUpdateDelta => Time.deltaTime;
    private float RawFixedDelta => Time.fixedDeltaTime;

    // 절대시간
    public static float absoluteN => Ready ? _i.RawUpdateDelta : 0f;
    public static float absoluteF => Ready ? _i.RawFixedDelta : 0f;// 근데 픽스드를써야만할까? 흠..

    // 월드
    private float TimeScale = 1f;
    public static float Dtime => Ready ? _i.RawUpdateDelta * _i.TimeScale : 0f;
    public static float FDtime => Ready ? _i.RawFixedDelta * _i.TimeScale : 0f;

    // 이벤트 전용(안쓸수도?)
    private float eventScale = 1f;
    public static float ETime => Ready ? _i.RawUpdateDelta * _i.eventScale : 0f;
    public static void SetScale_W(float value)
    {
        if(Ready) _i.TimeScale = Mathf.Max(0f, value);
    }
    public static void SetScale_E(float value)
    {
        if(Ready) _i.eventScale = Mathf.Max(0f, value);
    }

    public static float GetScale_W() => Ready ? _i.TimeScale : 1f;
    public static float GetScale_E() => Ready ? _i.eventScale : 1f;
    
}