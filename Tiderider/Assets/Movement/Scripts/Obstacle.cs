// Obstacle.cs

using UnityEditor.U2D;
using UnityEngine;



/// <summary>
/// Base class for obstacles. Handles random sprite assignment, collider setup, and downward movement.
/// </summary>
/// <remarks>
/// Maintained by: Obstacle System
/// </remarks>
/// 
[RequireComponent(typeof(SpriteRenderer))]
public class Obstacle : MonoBehaviour
{
    public string prefabId;
    private int spriteIndex;

    public int getSpriteNo()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        for(int i=0;i<sprites.Length;i++)
        {
            Sprite s = sprites[i];
            if(s.GetSpriteID() == sr.sprite.GetSpriteID())
            {
                return i;
            }
        }
        return 0;
    }

    public void SetSpriteIndex(int index)
    {
        this.spriteIndex = index;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprites[index];
        UpdateColliderToMatchSprite(sr.sprite);
    }


    [SerializeField] private float speed = 3f; // Match your ship's forwardSpeed
    [SerializeField] private Sprite[] sprites;

    /// <summary>
    /// Assigns a random sprite and updates collider to match.
    /// </summary>
    protected virtual void Start()
    {

    }

    /// <summary>
    /// Moves the obstacle downward.
    /// </summary>
    protected virtual void Update()
    {
        // Move downward
        transform.position += Vector3.down * speed * Time.deltaTime;
    }

    /// <summary>
    /// Updates the CircleCollider2D to fit the current sprite.
    /// </summary>
    /// <param name="sprite">The sprite to fit the collider to.</param>
    protected virtual void UpdateColliderToMatchSprite(Sprite sprite)
    {
        // Remove existing CircleCollider2D if present
        var existingCollider = GetComponent<CircleCollider2D>();
        if (existingCollider != null)
        {
            Destroy(existingCollider);
        }

        // Add a new CircleCollider2D and fit it to the sprite's bounds
        var newCollider = gameObject.AddComponent<CircleCollider2D>();
        if (sprite != null)
        {
            // Set the radius to fit the sprite's bounds (half the max of width/height)
            float pixelsPerUnit = sprite.pixelsPerUnit;
            Vector2 size = sprite.bounds.size;
            newCollider.radius = Mathf.Max(size.x, size.y) / 2f;
            newCollider.offset = sprite.bounds.center;
        }
    }

    /// <summary>
    /// Destroys the obstacle when it goes off-screen.
    /// </summary>
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}