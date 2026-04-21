//using UnityEngine;

//public class BulletSpawner : MonoBehaviour
//{
//    [SerializeField] private WeaponStat[] armory;

//    private float[] lastFired;
//    private Rigidbody2D rb;
//    private Collider2D col;

//    // target (player)
//    private Rigidbody2D playerRb;

//    void Start()
//    {
//        rb = GetComponent<Rigidbody2D>();
//        col = GetComponent<Collider2D>();

//        // pirates do NOT use TempWeaponManager like the player
//        // you will set armory in the Inspector

//        lastFired = new float[armory.Length];
//        ResetTimes();

//        var p = GameObject.FindGameObjectWithTag("Player");
//        if (p != null) playerRb = p.GetComponent<Rigidbody2D>();
//    }

//    public void ResetTimes()
//    {
//        for (int i = 0; i < lastFired.Length; i++)
//            lastFired[i] = Time.time;
//    }

//    private void FixedUpdate()
//    {
//        if (playerRb == null)
//        {
//            var p = GameObject.FindGameObjectWithTag("Player");
//            if (p != null) playerRb = p.GetComponent<Rigidbody2D>();
//            if (playerRb == null) return;
//        }

//        var monsterObj = GameObject.FindGameObjectWithTag("LevelMonster");
//        var monster = monsterObj ? monsterObj.GetComponent<Collider2D>() : null;

//        for (int i = 0; i < armory.Length; i++)
//        {
//            if (armory[i].weaponInfo == null) continue;

//            if (Time.time - lastFired[i] < armory[i].WeaponLevel.fireRate) continue;
//            lastFired[i] = Time.time;

//            var currentBullet = Instantiate(armory[i].weaponInfo.projectilePrefab).GetComponent<Bullet>();

//            currentBullet.Weapon = armory[i].weaponInfo;
//            currentBullet.Level = armory[i].level;
//            currentBullet.WeaponLevel = armory[i].WeaponLevel;
//            currentBullet.PlayerBullet = false; // pirate bullet

//            currentBullet.transform.position = Weapon.BulletOffsets[i] + transform.position;

//            // ----- predictive aim -----
//            Vector2 shooterPos = rb.position;
//            Vector2 targetPos = playerRb.position;
//            Vector2 targetVel = playerRb.linearVelocity;

//            // Bullet.cs uses: direction * WeaponLevel.speedOfBullet * 5f + shipSpeed
//            float bulletSpeed = armory[i].WeaponLevel.speedOfBullet * 5f;

//            Vector2 dir = Intercept2D.GetLeadDirection(shooterPos, targetPos, targetVel, bulletSpeed);

//            currentBullet.Activate(dir, rb.linearVelocity);
//            // --------------------------

//            Physics2D.IgnoreCollision(currentBullet.circleCollider, col, true);
//            if (monster != null)
//                Physics2D.IgnoreCollision(currentBullet.circleCollider, monster, true);

//            if (armory[i].weaponInfo.spawningSound != null)
//                AudioSource.PlayClipAtPoint(armory[i].weaponInfo.spawningSound, transform.position);
//        }
//    }
//}
