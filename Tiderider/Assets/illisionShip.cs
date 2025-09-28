using UnityEngine;

public class illisionShip : MonoBehaviour
{
    private SpriteRenderer playAreaRenderer;
    private Bounds playAreaBounds;
    private Transform playAreaTransform;

    // Glow effect variables
    private bool isGlowing = false;
    private float glowTimer = 0f;
    [SerializeField] private float glowDuration = 1f;
    private Color originalColor;
    [SerializeField] private Color glowColor = Color.yellow;

    // Player tracking variables
    [SerializeField] private string playerShipName = "PlayerShip";
    [SerializeField] private float xApproachSpeed = 2f; // Units per second

    private Transform playerShipTransform;

    // Blink sound variables
    [SerializeField] private AudioClip blinkClip;
    [SerializeField] private AudioSource audioSource;

    void Start()
    {
        // Find LevelManager and PlayArea
        GameObject levelManager = GameObject.Find("LevelManager");
        if (levelManager != null)
        {
            playAreaTransform = levelManager.transform.Find("PlayArea");
            if (playAreaTransform != null)
            {
                playAreaRenderer = playAreaTransform.GetComponent<SpriteRenderer>();
            }
            else
            {
                Debug.LogWarning("PlayArea child not found under LevelManager.");
            }
        }
        else
        {
            Debug.LogWarning("LevelManager object not found in the scene.");
        }

        // Store original color for glow effect
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            originalColor = sr.color;

        // Find player ship
        GameObject playerShip = GameObject.Find(playerShipName);
        if (playerShip != null)
        {
            playerShipTransform = playerShip.transform;
        }
        else
        {
            Debug.LogWarning("Player ship not found in the scene.");
        }

        // Get AudioSource if not set
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (playAreaRenderer == null)
            return;

        // Update bounds every frame in case PlayArea moves or scales
        playAreaBounds = playAreaRenderer.bounds;

        float y = transform.position.y;
        float bottomQuarter = Mathf.Lerp(playAreaBounds.min.y, playAreaBounds.max.y, 0.25f);

        // Move x closer to player ship if available and above player
        if (playerShipTransform != null && transform.position.y > playerShipTransform.position.y)
        {
            float targetX = playerShipTransform.position.x;
            float newX = Mathf.MoveTowards(transform.position.x, targetX, xApproachSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);
        }

        if (isGlowing)
        {
            glowTimer += Time.deltaTime;
            if (glowTimer >= glowDuration)
            {
                // Teleport to a random position in the top 1/4 of PlayArea
                float topQuarterMin = Mathf.Lerp(playAreaBounds.min.y, playAreaBounds.max.y, 0.75f);
                float topQuarterMax = playAreaBounds.max.y;
                float newY = Random.Range(topQuarterMin, topQuarterMax);
                float newX = Random.Range(playAreaBounds.min.x, playAreaBounds.max.x);

                transform.position = new Vector3(newX, newY, transform.position.z);

                // Play blink sound
                if (audioSource != null && blinkClip != null)
                {
                    audioSource.PlayOneShot(blinkClip);
                }

                // End glow
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = originalColor;

                isGlowing = false;
                glowTimer = 0f;
            }
        }
        else
        {
            // If ship enters the bottom 1/4 of PlayArea
            if (y <= bottomQuarter)
            {
                // Start glow
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.color = glowColor;

                isGlowing = true;
                glowTimer = 0f;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}