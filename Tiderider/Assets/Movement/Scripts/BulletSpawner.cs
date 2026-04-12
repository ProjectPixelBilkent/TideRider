using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private WeaponStat[] armory;
    [SerializeField] public SceneObjectSpawner objectSpawner;

    private float[] lastFired;
    private Rigidbody2D rb;
    private Collider2D col;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (CompareTag("Player"))
        {
            var playerArmory = TempWeaponManager.Instance.GetPlayerArmory();
            armory = new WeaponStat[playerArmory.Length];
            for (int i=0; i<armory.Length; i++)
            {
                armory[i] = new WeaponStat(playerArmory[i], 0);
            }
        }

        lastFired = new float[armory.Length];
        ResetTimes();
    }

    public void ResetTimes()
    {
        for(int i=0; i<lastFired.Length; i++)
        {
            lastFired[i] = Time.time;
        }
    }

    private void FixedUpdate()
    {
        if(objectSpawner!=null && !objectSpawner.isPausedForEnemy)
        {
            return;
        }
        var monster = GameObject.FindGameObjectWithTag("LevelMonster").GetComponent<Collider2D>();
        for(int i=0; i< armory.Length ; i++)
        {
            var weaponStat = armory[i];
            if (weaponStat.weaponInfo == null)
            {
                continue;
            }

            var weaponLevel = weaponStat.WeaponLevel;
            if (weaponLevel == null)
            {
                Debug.LogWarning($"Skipping slot {i} on '{gameObject.name}': weapon '{weaponStat.weaponInfo.weaponName}' has no valid weapon level.");
                continue;
            }

            if(Time.time - lastFired[i] < GetWeaponCooldown(weaponStat, weaponLevel))
            {
                continue;
            }

            lastFired[i] = Time.time;

            //Might be better to implement an object pool.
            if (weaponStat.weaponInfo.projectilePrefab == null)
            {
                Debug.LogWarning($"Skipping slot {i} on '{gameObject.name}': weapon '{weaponStat.weaponInfo.weaponName}' has no projectile prefab.");
                continue;
            }

            var currentBullet = Instantiate(weaponStat.weaponInfo.projectilePrefab).GetComponent<Bullet>();
            if (currentBullet == null)
            {
                Debug.LogWarning($"Skipping slot {i} on '{gameObject.name}': prefab '{weaponStat.weaponInfo.projectilePrefab.name}' has no Bullet component.");
                continue;
            }

            currentBullet.Weapon = weaponStat.weaponInfo;
            currentBullet.Level = weaponStat.level;
            currentBullet.WeaponLevel = weaponLevel;
            currentBullet.PlayerBullet = CompareTag("Player");
            currentBullet.OwnerTransform = transform;

            currentBullet.transform.position = Weapon.BulletOffsets[i] + transform.position;
            currentBullet.Activate(Weapon.BulletDirections[i], rb.linearVelocity);

            if (currentBullet.circleCollider != null)
            {
                Physics2D.IgnoreCollision(currentBullet.circleCollider, col, true);
                Physics2D.IgnoreCollision(currentBullet.circleCollider, monster, true);
            }

            if (weaponStat.weaponInfo.spawningSound != null)
            {
                AudioSource.PlayClipAtPoint(weaponStat.weaponInfo.spawningSound, transform.position);
            }
        }
    }

    private float GetWeaponCooldown(WeaponStat weaponStat, WeaponLevel weaponLevel)
    {
        if (weaponStat.weaponInfo == null || weaponLevel == null)
        {
            return 0f;
        }

        if (weaponStat.weaponInfo.projectilePrefab != null)
        {
            var flamethrower = weaponStat.weaponInfo.projectilePrefab.GetComponent<Flamethrower>();
            if (flamethrower != null)
            {
                return flamethrower.ShootDurationSeconds + flamethrower.PauseDurationSeconds;
            }

            var iceThrower = weaponStat.weaponInfo.projectilePrefab.GetComponent<IceThrower>();
            if (iceThrower != null)
            {
                return iceThrower.ShootDurationSeconds + iceThrower.PauseDurationSeconds;
            }
        }

        return weaponLevel.fireRate;
    }
}
