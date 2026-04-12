using DG.Tweening;
using UnityEngine;

public class ShockedState : State
{
    private Jellyfish jellyfish;
    private float rechargeTimer;

    public ShockedState(StateMachine machine) : base(machine) { }

    public override void Enter()
    {
        jellyfish = machine.Enemy as Jellyfish;

        if (jellyfish.playerTarget != null)
        {
            if (jellyfish.lightningPrefab != null)
            {
                Vector3 startPos = jellyfish.transform.position;
                Vector3 targetPos = jellyfish.playerTarget.transform.position;
                float distance = Vector3.Distance(startPos, targetPos);
                float duration = Mathf.Max(0.01f, distance / jellyfish.lightningSpeed);

                GameObject bolt = Object.Instantiate(jellyfish.lightningPrefab, startPos, Quaternion.identity);

                // Orient the bolt toward the player
                Vector2 dir = (targetPos - startPos).normalized;
                bolt.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

                Player playerRef = jellyfish.playerTarget;
                int damage = jellyfish.shockDamage;

                bolt.transform.DOMove(targetPos, duration)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        playerRef.TakeDamage(damage);
                        Object.Destroy(bolt);
                    });
            }
            else
            {
                // Fallback: deal damage immediately if no prefab is assigned
                jellyfish.playerTarget.TakeDamage(jellyfish.shockDamage);
            }
        }

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
