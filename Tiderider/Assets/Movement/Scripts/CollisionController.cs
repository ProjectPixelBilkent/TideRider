using UnityEngine;

public class CollisionController : MonoBehaviour
{
    [Header("Deceleration Settings")]
    [SerializeField] private float collisionDeceleration = 10f; // How much to decelerate on collision

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Called when this collider/rigidbody has begun touching another rigidbody/collider
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("obstacle"))
        {
            if (rb != null)
            {
                // Reduce velocity by deceleration factor
                rb.linearVelocity = rb.linearVelocity / collisionDeceleration;
            }
        }
    }
}
