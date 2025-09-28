using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public CircleCollider2D circleCollider;
    public Rigidbody2D rigidBody;

    public Weapon Weapon { get; set; }
    public int Level {  get; set; }
    public WeaponLevel WeaponLevel { get; set; }
    public bool PlayerBullet { get; set; }

    private Vector3 direction;
    private Vector3 shipSpeed;

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        print(direction + ":" + shipSpeed);
        rigidBody.linearVelocity = (direction * WeaponLevel.speedOfBullet * 5f + shipSpeed);
    }

    public void Activate(Vector3 direction, Vector3 shipSpeed)
    {
        direction = direction.normalized;

        spriteRenderer.enabled = true;
        circleCollider.enabled = true;

        this.direction = direction;
        this.shipSpeed = shipSpeed;
    }

    private void OnBecameInvisible()
    {
        transform.DOKill();
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.gameObject.name == "LevelManager")
        {
            Destroy(gameObject);
        }
    }
}
