using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class GhostShip : Enemy
{
    [Header("References")]
    public Transform playerTransform;
    public SpriteRenderer shipSprite;
    public Collider2D shipCollider;

    [Header("Spawn / Despawn")]
    public float topSpawnBandHeightRatio = 0.2f;
    public float sideSpawnInset = 1.5f;
    public float headingWallSafetyInset = 2.5f;
    public float disappearDelay = 1f;
    public float respawnDelay = 2f;
    public float fadeDuration = 0.35f;

    [Header("Movement")]
    public float sailSpeed = 2.5f;
    public float sailTime = 3f;

    [Tooltip("If the ship gets closer than this to the player, it disappears immediately.")]
    public float minDistanceToPlayer = 3f;

    [Tooltip("If the ship goes below the player by more than this amount, it disappears immediately.")]
    public float maxDistanceBelowPlayer = 4f;

    public float minTravelAngle = -45f;
    public float maxTravelAngle = 45f;
    public float noseAngleOffset = 90f;

    [Header("Combat")]
    public Weapon weaponData;
    public int weaponLevel = 0;
    public float fireCooldown = 0.8f;
    public int shotsPerCycle = 2;

    [Tooltip("Smaller = more accurate.")]
    public float aimRandomness = 0.005f;

    [Tooltip("How many times to refine the intercept prediction.")]
    public int aimIterations = 6;

    [Tooltip("Multiplier for how far ahead to lead the player. 1 = normal, >1 = more aggressive lead.")]
    public float extraAimAheadMultiplier = 6.87f;

    [Tooltip("Actual projectile world speed for the ghost ship. Uses the shared bullet system internally.")]
    public float ghostBulletSpeed = 8f;
    public float fallbackBulletSpeed = 8f;
    public float minimumAimDistance = 0.1f;
    public float bulletIgnoreDuration = 0.3f;
    public Color bulletTint = Color.cyan;

    [Header("Camera")]
    public float cameraPadding = 0.5f;
    public float despawnBelowCameraMargin = 0.75f;

    private Camera mainCamera;
    private Coroutine mainRoutine;
    private Vector2 currentSailDirection;
    private Vector2 hiddenPosition;
    private Color shipBaseColor = Color.white;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        shipCollider = GetComponent<Collider2D>();
        shipSprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (shipCollider == null) shipCollider = GetComponent<Collider2D>();
        if (shipSprite == null) shipSprite = GetComponentInChildren<SpriteRenderer>();

        if (shipSprite != null)
            shipBaseColor = shipSprite.color;

        mainCamera = Camera.main;
    }

    protected override void Start()
    {
        base.Start();
        FindPlayerIfNeeded();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }

        UpdateHiddenPosition();
        mainRoutine = StartCoroutine(MainLoop());
    }

    private void FindPlayerIfNeeded()
    {
        if (playerTransform != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    private IEnumerator MainLoop()
    {
        while (true)
        {
            yield return StartCoroutine(DisappearInstant());

            yield return new WaitForSeconds(respawnDelay);

            SpawnAtTopSideInsideCamera();
            yield return StartCoroutine(FadeShipIn());

            yield return StartCoroutine(SailAndShootRoutine());

            yield return new WaitForSeconds(disappearDelay);
        }
    }

    private void SpawnAtTopSideInsideCamera()
    {
        if (mainCamera == null || rb == null)
            return;

        FindPlayerIfNeeded();
        GetCameraBounds(out float minX, out float maxX, out float minY, out float maxY);

        float topBandHeight = Mathf.Max(0f, (maxY - minY) * Mathf.Clamp01(topSpawnBandHeightRatio));
        float spawnYMin = maxY - topBandHeight;
        float spawnY = Random.Range(spawnYMin, maxY);

        float travelAngle = Random.Range(minTravelAngle, maxTravelAngle);
        float headingRadians = travelAngle * Mathf.Deg2Rad;
        currentSailDirection = new Vector2(Mathf.Sin(headingRadians), -Mathf.Cos(headingRadians)).normalized;

        float spawnMinX = minX + sideSpawnInset;
        float spawnMaxX = maxX - sideSpawnInset;

        if (currentSailDirection.x > 0f)
            spawnMaxX = Mathf.Min(spawnMaxX, maxX - headingWallSafetyInset);
        else if (currentSailDirection.x < 0f)
            spawnMinX = Mathf.Max(spawnMinX, minX + headingWallSafetyInset);

        if (spawnMinX > spawnMaxX)
        {
            float centerX = (minX + maxX) * 0.5f;
            spawnMinX = centerX;
            spawnMaxX = centerX;
        }

        float spawnX = Random.Range(spawnMinX, spawnMaxX);

        Vector2 spawnPos = new Vector2(spawnX, spawnY);
        rb.position = spawnPos;
        SetShipRotationFromDirection(currentSailDirection);

        SetShipVisibility(0f, true);
        if (shipCollider != null) shipCollider.enabled = false;
    }

    private IEnumerator SailAndShootRoutine()
    {
        float timer = 0f;
        float shootTimer = 0f;
        int shotsFired = 0;

        SetShipRotationFromDirection(currentSailDirection);

        while (timer < sailTime)
        {
            timer += Time.deltaTime;
            shootTimer += Time.deltaTime;

            if (rb != null)
            {
                Vector2 nextPos = rb.position + currentSailDirection * sailSpeed * Time.deltaTime;
                rb.MovePosition(nextPos);
            }

            if (ShouldDisappearNow())
            {
                yield return StartCoroutine(DisappearInstant());
                yield break;
            }

            if (shotsFired < shotsPerCycle && shootTimer >= fireCooldown)
            {
                FireCannon();
                shotsFired++;
                shootTimer = 0f;
            }

            yield return null;
        }

        yield return StartCoroutine(DisappearInstant());
    }

    private bool ShouldDisappearNow()
    {
        if (rb == null)
            return false;

        if (HasHitSideWall())
            return true;

        if (IsBelowCameraDespawnLine())
            return true;

        if (playerTransform == null)
            return false;

        float distanceToPlayer = Vector2.Distance(rb.position, playerTransform.position);
        if (distanceToPlayer < minDistanceToPlayer)
            return true;

        float amountBelowPlayer = playerTransform.position.y - rb.position.y;
        if (amountBelowPlayer > maxDistanceBelowPlayer)
            return true;

        return false;
    }

    private bool HasHitSideWall()
    {
        if (mainCamera == null)
            return false;

        GetCameraBounds(out float minX, out float maxX, out _, out _);
        return rb.position.x <= minX || rb.position.x >= maxX;
    }

    private bool IsBelowCameraDespawnLine()
    {
        if (mainCamera == null)
            return false;

        GetCameraBounds(out _, out _, out float minY, out _);
        return rb.position.y < minY - despawnBelowCameraMargin;
    }

    private IEnumerator DisappearInstant()
    {
        if (shipCollider != null) shipCollider.enabled = false;

        if (shipSprite != null && shipSprite.enabled && fadeDuration > 0f)
            yield return StartCoroutine(FadeShipAlpha(shipSprite.color.a, 0f));

        SetShipVisibility(0f, false);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            UpdateHiddenPosition();
            rb.position = hiddenPosition;
        }
    }

    private void SetShipRotationFromDirection(Vector2 direction)
    {
        if (rb == null || direction.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + noseAngleOffset;
        rb.rotation = angle;
    }

    private WeaponLevel GetCurrentWeaponLevel()
    {
        if (weaponData == null || weaponData.weaponLevels == null || weaponData.weaponLevels.Length == 0)
            return null;

        int index = Mathf.Clamp(weaponLevel, 0, weaponData.weaponLevels.Length - 1);
        return weaponData.weaponLevels[index];
    }

    private float GetBulletSpeed()
    {
        if (ghostBulletSpeed > 0f)
            return ghostBulletSpeed;

        WeaponLevel level = GetCurrentWeaponLevel();
        if (level == null || level.speedOfBullet <= 0f)
            return fallbackBulletSpeed;

        return level.speedOfBullet * 5f;
    }

    private Vector2 GetPlayerVelocity()
    {
        if (playerTransform == null)
            return Vector2.zero;

        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRb != null)
            return playerRb.linearVelocity;

        return Vector2.zero;
    }

    private Vector2 GetPredictedAimTarget(Vector2 spawnPos)
    {
        if (playerTransform == null)
            return spawnPos;

        Vector2 playerPos = playerTransform.position;
        Vector2 playerVel = GetPlayerVelocity();
        float bulletSpeed = GetBulletSpeed();

        if (bulletSpeed <= 0.001f)
            return playerPos;

        Vector2 predicted = playerPos;

        for (int i = 0; i < aimIterations; i++)
        {
            float distance = Vector2.Distance(spawnPos, predicted);
            float travelTime = distance / bulletSpeed;

            Vector2 next = playerPos + playerVel * travelTime * extraAimAheadMultiplier;

            if ((next - predicted).sqrMagnitude < 0.0001f)
                break;

            predicted = next;
        }

        if (aimRandomness > 0f)
            predicted += Random.insideUnitCircle * aimRandomness;

        return predicted;
    }

    private bool FireCannon()
    {
        if (weaponData == null || weaponData.projectilePrefab == null)
            return false;

        WeaponLevel level = GetCurrentWeaponLevel();
        if (level == null)
            return false;

        Vector2 spawnPos = rb.position;
        Vector2 aimTarget = GetPredictedAimTarget(spawnPos);

        Vector2 dir = aimTarget - spawnPos;

        if (dir.magnitude < minimumAimDistance)
        {
            if (playerTransform != null)
                dir = ((Vector2)playerTransform.position - spawnPos).normalized;
            else
                dir = Vector2.down;
        }
        else
        {
            dir.Normalize();
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        GameObject bulletObj = Instantiate(
            weaponData.projectilePrefab,
            spawnPos,
            Quaternion.Euler(0f, 0f, angle)
        );

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Weapon = weaponData;
            bullet.WeaponLevel = CreateGhostWeaponLevel(level);
            bullet.PlayerBullet = false;
            bullet.Activate(dir, rb != null ? rb.linearVelocity : Vector2.zero);

            if (bullet.spriteRenderer != null)
                bullet.spriteRenderer.color = bulletTint;
        }

        Collider2D bulletCollider = bulletObj.GetComponent<Collider2D>();
        if (bulletCollider != null && shipCollider != null)
        {
            Physics2D.IgnoreCollision(bulletCollider, shipCollider, true);
            StartCoroutine(RestoreCollisionAfterDelay(bulletCollider, shipCollider, bulletIgnoreDuration));
        }

        return true;
    }

    private IEnumerator RestoreCollisionAfterDelay(Collider2D bulletCollider, Collider2D ownerCollider, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bulletCollider != null && ownerCollider != null)
            Physics2D.IgnoreCollision(bulletCollider, ownerCollider, false);
    }

    private WeaponLevel CreateGhostWeaponLevel(WeaponLevel baseLevel)
    {
        if (baseLevel == null)
            return null;

        WeaponLevel ghostLevel = new WeaponLevel
        {
            cost = baseLevel.cost,
            damage = baseLevel.damage,
            fireRate = baseLevel.fireRate,
            speedOfBullet = GetBulletLevelSpeed(),
            range = baseLevel.range,
            HP = baseLevel.HP,
            duration = baseLevel.duration
        };

        return ghostLevel;
    }

    private float GetBulletLevelSpeed()
    {
        float actualSpeed = GetBulletSpeed();
        return actualSpeed / 5f;
    }

    private void GetCameraBounds(out float minX, out float maxX, out float minY, out float maxY)
    {
        if (mainCamera == null)
        {
            minX = maxX = minY = maxY = 0f;
            return;
        }

        float camH = mainCamera.orthographicSize;
        float camW = camH * mainCamera.aspect;
        Vector2 camPos = mainCamera.transform.position;

        minX = camPos.x - camW + cameraPadding;
        maxX = camPos.x + camW - cameraPadding;
        minY = camPos.y - camH + cameraPadding;
        maxY = camPos.y + camH - cameraPadding;
    }

    private void UpdateHiddenPosition()
    {
        if (mainCamera == null)
        {
            hiddenPosition = new Vector2(0f, 999f);
            return;
        }

        GetCameraBounds(out float minX, out float maxX, out _, out float maxY);
        hiddenPosition = new Vector2((minX + maxX) * 0.5f, maxY + 8f);
    }

    private IEnumerator FadeShipIn()
    {
        if (shipSprite == null)
            yield break;

        SetShipVisibility(0f, true);

        if (fadeDuration > 0f)
            yield return StartCoroutine(FadeShipAlpha(0f, 1f));
        else
            SetShipVisibility(1f, true);

        if (shipCollider != null)
            shipCollider.enabled = true;
    }

    private IEnumerator FadeShipAlpha(float fromAlpha, float toAlpha)
    {
        if (shipSprite == null)
            yield break;

        float duration = Mathf.Max(0.0001f, fadeDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetShipVisibility(Mathf.Lerp(fromAlpha, toAlpha, t), true);
            yield return null;
        }

        SetShipVisibility(toAlpha, true);
    }

    private void SetShipVisibility(float alpha, bool visible)
    {
        if (shipSprite == null)
            return;

        Color color = shipBaseColor;
        color.a = Mathf.Clamp01(alpha) * shipBaseColor.a;
        shipSprite.color = color;
        shipSprite.enabled = visible;
    }
}
