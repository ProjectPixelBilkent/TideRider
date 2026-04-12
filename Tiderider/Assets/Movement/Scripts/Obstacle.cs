using UnityEngine;

/// <summary>
/// Base class for obstacles. Handles sprite assignment, collider setup, and downward movement.
/// </summary>
/// <remarks>
/// Maintained by: Obstacle System
/// </remarks>
[RequireComponent(typeof(SpriteRenderer))]
public class Obstacle : MonoBehaviour
{
    public enum TerrainType
    {
        General,
        Ice,
        Misty
    }

    public string prefabId;

    [SerializeField] private float speed = 3f;
    [SerializeField] protected int damageAmount = 10;

    [Header("Sprites By Terrain")]
    [SerializeField] private Sprite[] generalSprites;
    [SerializeField] private Sprite[] iceSprites;
    [SerializeField] private Sprite[] mistySprites;

    [SerializeField] private TerrainType typeOfTerrain = TerrainType.General;

    private int spriteIndex;

    public TerrainType TypeOfTerrain => typeOfTerrain;

    public TerrainType GetTerrainTypeFromCurrentSprite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
            return TerrainType.General;

        return GetTerrainTypeFromSpriteName(sr.sprite.name);
    }

    public void SetTerrainType(TerrainType terrainType)
    {
        typeOfTerrain = terrainType;
    }

    public void SetSpriteIndex(int index)
    {
        spriteIndex = index;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null)
            return;

        Sprite[] terrainSprites = GetSpritesByTerrain(typeOfTerrain);
        if (terrainSprites == null || terrainSprites.Length == 0)
        {
            Debug.LogWarning($"{name}: No sprites assigned for terrain type {typeOfTerrain}.");
            return;
        }

        index = Mathf.Clamp(index, 0, terrainSprites.Length - 1);
        spriteIndex = index;

        sr.sprite = terrainSprites[index];
        UpdateColliderToMatchSprite(sr.sprite);
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;
    }

    protected virtual void UpdateColliderToMatchSprite(Sprite sprite)
    {
        var existingCircle = GetComponent<CircleCollider2D>();
        if (existingCircle != null)
            Destroy(existingCircle);

        var existingPoly = GetComponent<PolygonCollider2D>();
        if (existingPoly != null)
            Destroy(existingPoly);

        if (sprite == null)
            return;

        var newCollider = gameObject.AddComponent<PolygonCollider2D>();
        int pathCount = sprite.GetPhysicsShapeCount();
        newCollider.pathCount = pathCount;
        var pathPoints = new System.Collections.Generic.List<Vector2>();
        for (int i = 0; i < pathCount; i++)
        {
            pathPoints.Clear();
            sprite.GetPhysicsShape(i, pathPoints);
            newCollider.SetPath(i, pathPoints);
        }
    }

    private Sprite[] GetSpritesByTerrain(TerrainType terrainType)
    {
        switch (terrainType)
        {
            case TerrainType.Ice:
                return iceSprites;
            case TerrainType.Misty:
                return mistySprites;
            case TerrainType.General:
            default:
                return generalSprites;
        }
    }

    private TerrainType GetTerrainTypeFromSpriteName(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
            return TerrainType.General;

        string lowerName = spriteName.ToLowerInvariant();

        if (lowerName.Contains("ice"))
            return TerrainType.Ice;

        if (lowerName.Contains("misty"))
            return TerrainType.Misty;

        if (lowerName.Contains("general"))
            return TerrainType.General;

        return TerrainType.General;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            OnHitByPlayer(player);
        }
    }

    protected virtual void OnHitByPlayer(Player player)
    {
        player.TakeDamage(damageAmount);
        Destroy(gameObject);
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}