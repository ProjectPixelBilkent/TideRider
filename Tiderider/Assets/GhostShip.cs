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
    public bool spawnFromLeft = true;
    public float topSpawnInset = 1f;
    public float sideSpawnInset = 1.5f;
    public float disappearDelay = 1f;
    public float respawnDelay = 2f;

    [Header("Movement")]
    public float sailSpeed = 2.5f;
    public float sailTime = 3f;

    [Tooltip("If the ship gets closer than this to the player, it disappears immediately.")]
    public float minDistanceToPlayer = 3f;

    [Tooltip("If the ship goes below the player by more than this amount, it disappears immediately.")]
    public float maxDistanceBelowPlayer = 4f;

    public Vector2 leftSpawnDirection = new Vector2(1f, -0.5f);
    public Vector2 rightSpawnDirection = new Vector2(-1f, -0.5f);
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

    public float fallbackBulletSpeed = 8f;
    public float minimumAimDistance = 0.1f;
    public float bulletIgnoreDuration = 0.3f;

    [Header("Camera")]
    public float cameraPadding = 0.5f;

    private Camera mainCamera;
    private Coroutine mainRoutine;
    private Vector2 currentSailDirection;

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

        mainCamera = Camera.main;
    }

    private void Start()
    {
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }

        mainRoutine = StartCoroutine(MainLoop());
    }

    private IEnumerator MainLoop()
    {
        while (true)
        {
            yield return StartCoroutine(DisappearInstant());

            yield return new WaitForSeconds(respawnDelay);

            SpawnAtTopSideInsideCamera();

            yield return StartCoroutine(SailAndShootRoutine());

            yield return new WaitForSeconds(disappearDelay);
        }
    }

    private void SpawnAtTopSideInsideCamera()
    {
        if (mainCamera == null || rb == null)
            return;

        GetCameraBounds(out float minX, out float maxX, out float minY, out float maxY);

        float spawnX = spawnFromLeft
            ? minX + sideSpawnInset
            : maxX - sideSpawnInset;

        float spawnY = maxY - topSpawnInset;

        Vector2 spawnPos = new Vector2(spawnX, spawnY);
        rb.position = spawnPos;

        currentSailDirection = spawnFromLeft ? leftSpawnDirection.normalized : rightSpawnDirection.normalized;
        SetShipRotationFromDirection(currentSailDirection);

        if (shipSprite != null) shipSprite.enabled = true;
        if (shipCollider != null) shipCollider.enabled = true;

        spawnFromLeft = !spawnFromLeft;
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
        if (rb == null || playerTransform == null)
            return false;

        float distanceToPlayer = Vector2.Distance(rb.position, playerTransform.position);
        if (distanceToPlayer < minDistanceToPlayer)
            return true;

        float amountBelowPlayer = playerTransform.position.y - rb.position.y;
        if (amountBelowPlayer > maxDistanceBelowPlayer)
            return true;

        return false;
    }

    private IEnumerator DisappearInstant()
    {
        if (shipSprite != null) shipSprite.enabled = false;
        if (shipCollider != null) shipCollider.enabled = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        yield return null;
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
        WeaponLevel level = GetCurrentWeaponLevel();
        if (level == null || level.speedOfBullet <= 0f)
            return fallbackBulletSpeed;

        return level.speedOfBullet;
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
            bullet.WeaponLevel = level;
            bullet.PlayerBullet = false;
            bullet.Activate(dir, Vector2.zero);
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
}