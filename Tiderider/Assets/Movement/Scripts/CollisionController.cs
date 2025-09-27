// CollisionController.cs

using DG.Tweening;
using UnityEngine;

/// <summary>
/// Handles collision logic for the ship, including bounce-back and color flashing on obstacle collision.
/// Also applies big damage if colliding with a monster tagged "levelmonster".
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

    [Header("Monster Collision")]
    [SerializeField] private int monsterCollisionDamage = 50;
    [SerializeField] private float bounceForceMonster = 30f;

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
    /// Also handles collision with monster tagged "levelmonster".
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
            if (collision.gameObject.GetComponent<BigObstacle>() == null && collision.gameObject.GetComponent<Obstacle>() != null)
            {
                Destroy(collision.gameObject);
            }
        }
        else if (collision.gameObject.CompareTag("LevelMonster"))
        {
            // Take big damage on monster collision
            if (model != null)
            {

                Vector2 bounceDir = (transform.position - collision.transform.position).normalized;
                shipController.Bounce(bounceDir * bounceForceMonster);
                model.Decrement(monsterCollisionDamage);
            }

            // Flash color
            if (spriteRenderer != null && !isFlashing)
                StartCoroutine(FlashColor());
        }
        else if (collision.gameObject.CompareTag("Bullet"))
        {
            var b = collision.collider.GetComponent<Bullet>();
            b.Weapon.OnCollisionWithBullet(model, b.Level);
            b.transform.DOKill();
            Destroy(b.gameObject);
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

