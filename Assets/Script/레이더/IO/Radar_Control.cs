using UnityEngine;
using UnityEngine.InputSystem;

public class Radar_Control : MonoBehaviour
{
    private Radar_Core core;
    public Transform HeadOnBeam;
    public float ManualRotationSpeed;
    private float _inputDirection;
    
    public void Init(Radar_Core radarCore) => core = radarCore;
    

    void FixedUpdate()
    {
        if (_inputDirection != 0) core.MoveBeamAngle(_inputDirection);
        Updatebeam();
    }
    void Updatebeam() => core.UpdateBeamAngle(360f - (HeadOnBeam.localEulerAngles.z - 90f + 360f) % 360f);

    // --- 입력부 ---
    public void OnHeadOnWay(InputValue value) => _inputDirection = value.Get<float>();
    public void OnSelectBtn(InputValue value) => core.ChangePing((int)value.Get<float>());
    public void OnLockOnBtn(InputValue value) => core.HardLock();
    public void OnPowerBtn(InputValue value)  => core.Power_Radar();
    public void OnTWSOnBtn(InputValue value)  => core.Power_TWS();
    
    // --- 접근부 ---
    public void MoveHeadonbeam_Ctrl(float way) => HeadOnBeam.localRotation *= Quaternion.Euler(0, 0, way * ManualRotationSpeed * Time.deltaTime);

}
