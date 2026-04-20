using UnityEngine;

/// <summary>
/// A collectible coin that moves downward with the level and is collected when the player touches it.
/// Add this component to a coin prefab with a trigger CircleCollider2D.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class Coin : MonoBehaviour
{
    public enum CoinType
    {
        Silver = 10,
        Gold = 30
    };

    public string prefabId;
    [SerializeField] private CoinType coinValue;

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
            DataManager.IncrementCoinAmount((int) coinValue);
            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    public static int GetTotalCoinValue(TextAsset levelJsonFile)
    {
        return GetTotalCoinValue(levelJsonFile.text);
    }

    public static int GetTotalCoinValue(string levelJson)
    {
        SavedSceneData sceneData = JsonUtility.FromJson<SavedSceneData>(levelJson);
        int total = 0;
        foreach (SavedObjectData obj in sceneData.objects)
        {
            if (obj.objectType != SpawnObjectType.Coin) continue;

            switch (obj.prefabId)
            {
                case "gold_coin":
                    total += (int)CoinType.Gold;
                    break;
                case "silver_coin":
                    total += (int)CoinType.Silver;
                    break;
            }
        }
        return total;
    }
}
