using DG.Tweening;
using UnityEngine;

public class ShockedState : State
{
    private Jellyfish jellyfish;
    private float durationTimer;
    private float damageTimer;
    private float flashTimer;
    private bool showingFlashColor;

    public ShockedState(StateMachine machine) : base(machine) { }

    public override void Enter()
    {
        jellyfish = machine.Enemy as Jellyfish;
        flashTimer = 0f;
        damageTimer = 0f;
        showingFlashColor = false;

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

                bolt.transform.DOMove(targetPos, duration)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        Object.Destroy(bolt);
                    });
            }

            TryApplyShockDamage();
        }

        durationTimer = jellyfish.shockDuration;

        if (SoundLibrary.Instance != null)
            SoundLibrary.Instance.Play("jellyfish");

        jellyfish.StopMovementAnimation();
        jellyfish.SetAttackSprite();

        if (jellyfish.spriteRenderer != null)
        {
            jellyfish.spriteRenderer.color = jellyfish.attackFlashColorA;
        }
        jellyfish.SetRadiusColor(jellyfish.attackFlashColorA, jellyfish.attackFlashRadiusAlpha);
        jellyfish.SetAttackRingVisible(false, jellyfish.attackFlashColorB);

        // Stop moving during discharge
        jellyfish.rigidBody.linearVelocity = Vector2.zero;
    }

    public override void Exit()
    {
        jellyfish.StartMovementAnimation();
        jellyfish.StartShockCooldown();
        jellyfish.SetAttackRingVisible(false, jellyfish.attackFlashColorB);
    }

    public override void Update()
    {
        durationTimer -= Time.fixedDeltaTime;
        damageTimer += Time.fixedDeltaTime;
        flashTimer += Time.deltaTime;

        jellyfish.rigidBody.linearVelocityY = Camera.main.velocity.y;

        if (jellyfish.playerTarget != null)
        {
            if (jellyfish.shockDamageInterval <= 0f)
            {
                TryApplyShockDamage();
            }
            else
            {
                while (damageTimer >= jellyfish.shockDamageInterval)
                {
                    TryApplyShockDamage();
                    damageTimer -= jellyfish.shockDamageInterval;
                }
            }
        }

        if (jellyfish.spriteRenderer != null && jellyfish.attackFlashInterval > 0f)
        {
            while (flashTimer >= jellyfish.attackFlashInterval)
            {
                showingFlashColor = !showingFlashColor;
                Color flashColor = showingFlashColor ? jellyfish.attackFlashColorB : jellyfish.attackFlashColorA;
                jellyfish.spriteRenderer.color = flashColor;
                jellyfish.SetRadiusColor(flashColor, jellyfish.attackFlashRadiusAlpha);
                jellyfish.SetAttackRingVisible(showingFlashColor, jellyfish.attackFlashColorB);
                flashTimer -= jellyfish.attackFlashInterval;
            }
        }

        if (durationTimer <= 0f)
        {
            machine.ChangeState(new IdleState(machine));
        }
    }

    private void TryApplyShockDamage()
    {
        if (jellyfish.playerTarget == null)
        {
            return;
        }

        float sqrDist = ((Vector2)(jellyfish.playerTarget.transform.position - jellyfish.GetShockCenterWorld())).sqrMagnitude;
        if (sqrDist <= jellyfish.shockRadius * jellyfish.shockRadius)
        {
            jellyfish.playerTarget.TakeDamage(jellyfish.shockDamage);
        }
    }
}
