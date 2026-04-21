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

    private void LateUpdate()
    {
        EnforceCameraBounds();
    }

    private void FixedUpdate()
    {
        if (fsm != null)
            fsm.FixedUpdate();
    }

    private void EnforceCameraBounds()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = AspectRatioController.DesignedHalfWidth;
        Vector3 camPos = cam.transform.position;

        float leftBound = camPos.x - halfWidth;
        float rightBound = camPos.x + halfWidth;
        float bottomBound = camPos.y - halfHeight;

        Vector3 pos = transform.position;

        // Out of bounds at the bottom — kill the enemy
        if (pos.y < bottomBound)
        {
            TakeDamage(int.MaxValue);
            return;
        }

        // Out of bounds left or right — push back inside
        bool clamped = false;
        if (pos.x < leftBound)
        {
            pos.x = leftBound;
            clamped = true;
        }
        else if (pos.x > rightBound)
        {
            pos.x = rightBound;
            clamped = true;
        }

        if (clamped)
        {
            transform.position = pos;
            // Kill horizontal velocity so the enemy doesn't slide right back out
            if (rb != null)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
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