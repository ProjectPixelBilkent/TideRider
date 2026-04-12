using System;
using UnityEngine;

public class Enemy : HasHealth
{
    protected StateMachine fsm;
    protected Rigidbody2D rb;

    [SerializeField] protected float speed;
    protected int contactDamage;

    [Header("Spawner")]
    public string prefabId;

    public event Action<Enemy> OnEnemyDied;

    public Rigidbody2D rigidBody => rb;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();

        // Enemies should not collide with the level boundary (EdgeCollider2D on LevelManager)
        SceneObjectSpawner spawner = FindFirstObjectByType<SceneObjectSpawner>();
        if (spawner != null)
        {
            EdgeCollider2D boundary = spawner.GetComponent<EdgeCollider2D>();
            Collider2D myCollider = GetComponent<Collider2D>();
            if (boundary != null && myCollider != null)
                Physics2D.IgnoreCollision(myCollider, boundary, true);
        }
    }

    private void FixedUpdate()
    {
        if (fsm != null)
            fsm.FixedUpdate();
    }

    public override void Die()
    {
        if (currentHealth <= 0)
        {
            OnEnemyDied?.Invoke(this);
            Destroy(gameObject);
        }
    }
}