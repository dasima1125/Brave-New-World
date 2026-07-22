using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Signal : MonoBehaviour
{
    public float speed = 5f;
    public float range = 10f;
    public float currentAngle;

    private Rigidbody2D rb;

    private float startX;
    private int direction = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //rb.gravityScale = 0; // 중력 영향 차단
        //startX = transform.position.x;
    }
    /*

    void FixedUpdate()
    {
        if (transform.position.x > startX + range) direction = -1;
        else if (transform.position.x < startX - range) direction = 1;

        rb.linearVelocity = new Vector2(direction * speed, 0);

        currentAngle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
    }
    */
}

