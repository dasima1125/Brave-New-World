using UnityEngine;
using UnityEngine.InputSystem;

public class Turret_Control : MonoBehaviour
{
    Turret_Core _core;
    public void Init(Turret_Core core)
    {
        _core = core;
    }

    public void OnFireBtn(InputValue value) => _core.Fire();
    public void OnChangeBtn(InputValue value)
    {
        Vector2 scrollDelta = value.Get<Vector2>();
        //Debug.Log(scrollDelta);
        if (Mathf.Abs(scrollDelta.y) < 0.1f) return;
        int direction = scrollDelta.y > 0 ? 1 : -1;
        _core.ChangeAAM(direction * -1);
    }
}
