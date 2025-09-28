using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public CircleCollider2D circleCollider;

    public Weapon Weapon { get; set; }
    public int Level {  get; set; }
    public WeaponLevel WeaponLevel { get; set; }
    public bool PlayerBullet { get; set; }

    private Vector3 direction;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Activate(Vector3 direction)
    {
        direction = direction.normalized;

        spriteRenderer.enabled = true;
        circleCollider.enabled = true;

        int k = 20;
        transform.DOMove(transform.position + direction * k, k / WeaponLevel.speedOfBullet).onComplete += () =>
        {
            Destroy(gameObject);
        };
    }

    private void OnBecameInvisible()
    {
        transform.DOKill();
        Destroy(gameObject);
    }
}
