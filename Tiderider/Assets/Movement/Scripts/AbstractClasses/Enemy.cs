using UnityEngine;

public class Enemy: HasHealth
{
    protected StateMachine fsm;
    protected Rigidbody2D rb;
    [SerializeField] protected float speed;
    protected int contactDamage;

    public Rigidbody2D rigidBody { get { return rb; } }

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        fsm.FixedUpdate();
    }
}
