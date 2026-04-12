using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Snowball : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D coll;
    private Vector2 direction;
    private float speed;
    private int damage;

    [SerializeField] private float lifetime = 6f;
    [SerializeField] private float spinSpeed = 360f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
    }

    public void Init(Vector2 direction, float speed, int damage = 10)
    {
        this.direction = direction.normalized;
        this.speed = speed;
        this.damage = damage;

        CancelInvoke(nameof(DestroySelf));
        Invoke(nameof(DestroySelf), lifetime);
    }

    public void IgnoreCollider(Collider2D other)
    {
        if (coll != null && other != null)
        {
            Physics2D.IgnoreCollision(coll, other, true);
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            Vector2 screenVelocity = Camera.main != null ? Camera.main.velocity : Vector2.zero;
            rb.linearVelocity = direction * speed + screenVelocity;
        }
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out HasHealth health) && health.CompareTag("Player"))
        {
            health.TakeDamage(damage);
            DestroySelf();
            return;
        }

        if (collision.collider.gameObject.name == "LevelManager" || collision.collider.CompareTag("obstacle"))
        {
            DestroySelf();
        }
    }

    private void DestroySelf()
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
