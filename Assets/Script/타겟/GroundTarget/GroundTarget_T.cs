using UnityEngine;

public class GroundTarget_T : MonoBehaviour, IDamageable_Ally
{
    public bool IsDestroyed { get; private set; } = false;

    public void TakeDamage(float damage)
    {
        if (IsDestroyed) return;
        Debug.Log($"아군목표 피격당함 : 도시 피격");
        IsDestroyed = true;
        G_Excutor.Call("CityDestroyed");
    }


}
public interface IDamageable_Ally

{
    bool IsDestroyed { get; }
    void TakeDamage(float damage);
}