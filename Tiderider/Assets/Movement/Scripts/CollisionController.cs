// CollisionController.cs

using UnityEngine;

/// <summary>
/// Handles collision logic for the ship, including bounce-back and color flashing on obstacle collision.
/// </summary>
/// <remarks>
/// Maintained by: Collision System
/// </remarks>

public class CollisionController : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float bounceForce = 8f;
    [SerializeField] private float colorFlashDuration = 0.5f;
    [SerializeField] private Color flashColor = Color.white;


    [Header("Ship Modal")]
    [SerializeField] private ShipModel model;


    private ShipController shipController;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isFlashing = false;


    /// <summary>
    /// Initialize references.
    /// </summary>
    void Start()
    {
        shipController = GetComponent<ShipController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    /// <summary>
    /// Handles collision with obstacles, applies bounce and color flash, destroys obstacle if not big.
    /// </summary>
    /// <param name="collision">Collision data</param>
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("obstacle"))
        {
            // Bounce back
            if (shipController != null)
            {
                Vector2 bounceDir = (transform.position - collision.transform.position).normalized;
                shipController.Bounce(bounceDir * bounceForce);
                model.Decrement(10);
            }

            // Flash color
            if (spriteRenderer != null && !isFlashing)
                StartCoroutine(FlashColor());

            // Destroy the obstacle unless it's a BigObstacle
            var bigObstacle = collision.gameObject.GetComponent<BigObstacle>();
            if (bigObstacle == null)
            {
                Destroy(collision.gameObject);
            }
        }
    }

    /// <summary>
    /// Coroutine to flash the ship's color for a short duration.
    /// </summary>
    private System.Collections.IEnumerator FlashColor()
    {
        isFlashing = true;
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(colorFlashDuration);
        spriteRenderer.color = originalColor;
        isFlashing = false;
    }
}
