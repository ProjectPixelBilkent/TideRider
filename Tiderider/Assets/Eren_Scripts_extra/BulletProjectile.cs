using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    public Weapon cannon;
    public int level;
    public Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        
        BulletSpawn(cannon.weaponLevels[level].speedOfBullet);
        Destroy(gameObject, cannon.weaponLevels[level].duration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void BulletSpawn(float speed)
    {
        rb.linearVelocity = Vector2.up * speed;
    }
}
