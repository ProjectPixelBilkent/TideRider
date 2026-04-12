using UnityEngine;
#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
#endif

[RequireComponent(typeof(SpriteRenderer))]
public class Monster : MonoBehaviour
{
    [Header("Animation")]
    [Min(0f)] public float spriteSwapInterval = 0.12f;
    [Min(1)] public int specialCycleInterval = 20;
    public Sprite krakenAttack1;
    public Sprite krakenAttack2;
    public Sprite krakenAttack3;
    public Sprite krakenAttack4;
    public Sprite krakenAttack5;
    public Sprite krakenAttack6;
    public Sprite krakenAttack7;
    public Sprite krakenAttack8;

    private SpriteRenderer spriteRenderer;
    private float spriteTimer;
    private int spriteIndex;
    private int completedCycles;
    private bool useTwoOnNextSpecialCycle = true;
    private Sprite[] currentSequence;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteTimer = 0f;
        spriteIndex = 0;
        completedCycles = 0;
        currentSequence = BuildSequence();

        if (spriteRenderer != null && currentSequence.Length > 0)
        {
            spriteRenderer.sprite = currentSequence[0];
        }
    }

    void Update()
    {
        if (spriteRenderer == null || currentSequence == null || currentSequence.Length == 0)
        {
            return;
        }

        spriteTimer += Time.deltaTime;
        float interval = Mathf.Max(0.01f, spriteSwapInterval);

        while (spriteTimer >= interval)
        {
            spriteTimer -= interval;
            spriteIndex++;

            if (spriteIndex >= currentSequence.Length)
            {
                spriteIndex = 0;
                completedCycles++;

                if (completedCycles >= Mathf.Max(1, specialCycleInterval))
                {
                    completedCycles = 0;
                    bool useTwoThisCycle = useTwoOnNextSpecialCycle;
                    useTwoOnNextSpecialCycle = !useTwoOnNextSpecialCycle;
                    currentSequence = BuildSequence(true, useTwoThisCycle);
                }
                else
                {
                    currentSequence = BuildSequence();
                }
            }

            if (currentSequence.Length == 0)
            {
                return;
            }

            spriteRenderer.sprite = currentSequence[spriteIndex];
        }
    }

    private Sprite[] BuildSequence(bool includeSpecial = false, bool useAttackTwo = true)
    {
        var sequence = new System.Collections.Generic.List<Sprite>();
        AddIfPresent(sequence, krakenAttack1);
        if (includeSpecial && useAttackTwo)
        {
            AddIfPresent(sequence, krakenAttack2);
        }
        AddIfPresent(sequence, krakenAttack3);
        AddIfPresent(sequence, krakenAttack4);
        AddIfPresent(sequence, krakenAttack5);
        if (includeSpecial && !useAttackTwo)
        {
            AddIfPresent(sequence, krakenAttack6);
        }
        AddIfPresent(sequence, krakenAttack7);
        AddIfPresent(sequence, krakenAttack8);
        return sequence.ToArray();
    }

    private void AddIfPresent(System.Collections.Generic.List<Sprite> sequence, Sprite sprite)
    {
        if (sprite != null)
        {
            sequence.Add(sprite);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        print("Monster collided with " + collision.gameObject.name);
        HandlePlayerContact(collision.gameObject);
    }

    private void HandlePlayerContact(GameObject target)
    {
        Player player = target.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(player.MaxHealth);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Movement/RealAssets/kraken" });
        var spritesByPath = guids
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(path => path)
            .ToDictionary(path => path, AssetDatabase.LoadAssetAtPath<Sprite>);

        krakenAttack1 = GetSprite(spritesByPath, "Kraken attack 1.png", krakenAttack1);
        krakenAttack2 = GetSprite(spritesByPath, "Kraken attack 2.png", krakenAttack2);
        krakenAttack3 = GetSprite(spritesByPath, "Kraken attack 3.png", krakenAttack3);
        krakenAttack4 = GetSprite(spritesByPath, "Kraken attack 4.png", krakenAttack4);
        krakenAttack5 = GetSprite(spritesByPath, "Kraken attack 5.png", krakenAttack5);
        krakenAttack6 = GetSprite(spritesByPath, "Kraken attack 6.png", krakenAttack6);
        krakenAttack7 = GetSprite(spritesByPath, "Kraken attack 7.png", krakenAttack7);
        krakenAttack8 = GetSprite(spritesByPath, "Kraken attack 8.png", krakenAttack8);
    }

    private Sprite GetSprite(Dictionary<string, Sprite> spritesByPath, string fileName, Sprite current)
    {
        if (current != null)
        {
            return current;
        }

        string path = $"Assets/Movement/RealAssets/kraken/{fileName}";
        return spritesByPath.TryGetValue(path, out Sprite sprite) ? sprite : null;
    }
#endif
}
