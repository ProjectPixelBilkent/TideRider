using UnityEngine;

public class ShockedState : State
{
    private Jellyfish jellyfish;
    private float rechargeTimer;

    public ShockedState(StateMachine machine) : base(machine) { }

    public override void Enter()
    {
        jellyfish = machine.Enemy as Jellyfish;

        // Deal damage to player immediately on entering shock
        if (jellyfish.playerTarget != null)
            jellyfish.playerTarget.TakeDamage(jellyfish.shockDamage);

        rechargeTimer = jellyfish.shockCooldown;

        // Visual: dim the jellyfish while recharging
        if (jellyfish.spriteRenderer != null)
            jellyfish.spriteRenderer.color = jellyfish.rechargingColor;
        jellyfish.UpdateRadiusColor();

        // Stop moving during discharge
        jellyfish.rigidBody.linearVelocity = Vector2.zero;
    }

    public override void Exit()
    {
        // Restore charged glow when done recharging
        if (jellyfish.spriteRenderer != null)
            jellyfish.spriteRenderer.color = jellyfish.chargedColor;
        jellyfish.UpdateRadiusColor();
    }

    public override void Update()
    {
        rechargeTimer -= Time.fixedDeltaTime;

        // Keep the jellyfish drifting with the camera scroll but not actively moving
        jellyfish.rigidBody.linearVelocityY = Camera.main.velocity.y;

        if (rechargeTimer <= 0f)
        {
            jellyfish.isShockCharged = true;
            machine.ChangeState(new IdleState(machine));
        }
    }
}
