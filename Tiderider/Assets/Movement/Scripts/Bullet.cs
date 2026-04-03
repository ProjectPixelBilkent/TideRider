using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] public SpriteRenderer spriteRenderer;
    public CircleCollider2D circleCollider;
    public Rigidbody2D rigidBody;

    public Weapon Weapon { get; set; }
    public int Level {  get; set; }
    public WeaponLevel WeaponLevel { get; set; }
    public bool PlayerBullet { get; set; }

    protected Vector3 direction;
    protected Vector3 shipSpeed;

    // Update is called once per frame
    void Update()
    {
        
    }

    protected virtual void FixedUpdate()
    {
        rigidBody.linearVelocity = (direction * WeaponLevel.speedOfBullet * 5f + shipSpeed);
    }

    public virtual void Activate(Vector3 direction, Vector3 shipSpeed)
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

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.gameObject.name == "LevelManager")
        {
            Destroy(gameObject);
            return;
        }

        if (collision.collider.TryGetComponent(out HasHealth health))
        {
            if (ShouldDamageTarget(health))
            {
                health.TakeDamage(WeaponLevel.damage);
                transform.DOKill();
                Destroy(gameObject);
            }
        }
    }

    private bool ShouldDamageTarget(HasHealth health)
    {
        if (health == null)
        {
            return false;
        }

        if (PlayerBullet)
        {
            return !health.CompareTag("Player");
        }

        return health.CompareTag("Player");
    }
}
