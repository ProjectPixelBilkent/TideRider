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