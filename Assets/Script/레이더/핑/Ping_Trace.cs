using UnityEngine;

public class Ping_Trace : MonoBehaviour
{
    public void MovePing(Vector2 position, float rotation)
    {
        transform.SetPositionAndRotation(position, Quaternion.Euler(0, 0, rotation - 90f));
    }

    public void DestroyPing()
    {
        Destroy(gameObject);
    }
}
