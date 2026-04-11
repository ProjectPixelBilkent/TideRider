using UnityEngine;

/// <summary>
/// A collectible coin that moves downward with the level and is collected when the player touches it.
/// Add this component to a coin prefab with a trigger CircleCollider2D.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Coin : MonoBehaviour
{
    public string prefabId;
    [SerializeField] private int coinValue = 1;

    private void Awake()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            DataManager.IncrementCoinAmount(coinValue);
            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
