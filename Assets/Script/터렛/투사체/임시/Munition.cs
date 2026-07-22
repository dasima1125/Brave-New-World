using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Munition : MonoBehaviour
{
    public AnimationCurve ThrustCurve;

    private Rigidbody2D rb;
    private float elapsedTime;
    private float lifeTime;

    [Header("Debug")]
    [SerializeField] private float _currentSpeed;
    [SerializeField] private float _normalizedTime; // t값도 보여주면 커브 어디쯤인지 알 수 있어

    public void Init(float lifeTime)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        elapsedTime = 0f;
        this.lifeTime = lifeTime;
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        elapsedTime += Time.fixedDeltaTime;

        float t = elapsedTime / lifeTime;
        float currentSpeed = ThrustCurve.Evaluate(t) * 15f;

        _normalizedTime = t;
        _currentSpeed = currentSpeed;

        Vector2 dir = new(
            Mathf.Cos(rb.rotation * Mathf.Deg2Rad),
            Mathf.Sin(rb.rotation * Mathf.Deg2Rad)
        );
        rb.linearVelocity = dir * currentSpeed;
    }
}