using UnityEngine;

public class Snowball : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed = 5f;

    private Vector2 direction;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 dir, float newSpeed)
    {
        direction = dir.normalized;
        speed = newSpeed;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        rb.linearVelocity = direction * speed;
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}