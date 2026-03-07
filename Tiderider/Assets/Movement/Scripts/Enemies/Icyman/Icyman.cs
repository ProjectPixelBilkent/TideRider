using UnityEngine;

public class Icyman : Enemy
{
    [Header("Behavior")]
    public float xMaxDist = 5f;
    public float yMaxDist = 5f;
    public float screenEdgePadding = 0.5f;
    public float speedMultiplier = 0.3f;
    public float xSpeedMultiplier = 1.5f;
    public float ySmoothTime = 0.35f;
    public float idleDuration = 2f;
    public float attackRecovery = 0.5f;

    [Header("Close Range Attack")]
    public int swipeDamage = 10;
    public float swipeRange = 1.5f;
    public float swipeRadius = 0.9f;
    public LayerMask playerLayer;

    [Header("Snowball Attack")]
    public GameObject snowballPrefab;
    public Transform snowballSpawn;
    public float snowballSpeed = 6f;
    protected override void Start()
    {

        base.Start();

        fsm = new StateMachine();
        fsm.Init(new Idle(fsm, rb, speed), this);
    }

    public float topOffsetFromCamera = 3.5f; // same concept as jellyfish top
    public Vector2 GetCameraRelativeTarget(float targetX, float yOffsetFromTop)
    {
        float topY = Camera.main.transform.position.y + topOffsetFromCamera;
        float y = topY + yOffsetFromTop;   // yOffsetFromTop usually negative (below top)

        return new Vector2(targetX, y);
    }

    public float ClampToScreenX(float x)
    {
        var cam = Camera.main;
        float z = Mathf.Abs(cam.transform.position.z);
        float left = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, z)).x + screenEdgePadding;
        float right = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, z)).x - screenEdgePadding;
        return Mathf.Clamp(x, left, right);
    }

    public Transform FindPlayerTransform()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        return go != null ? go.transform : null;
    }

    public void DoSwipeAttack()
    {
        Vector2 origin = transform.position;
        Vector2 dir = Vector2.down;
        var player = FindPlayerTransform();
        if (player != null)
        {
            dir = ((Vector2)player.position - origin).normalized;
        }

        Vector2 center = origin + dir * swipeRange;
        Collider2D[] hits = playerLayer.value != 0
            ? Physics2D.OverlapCircleAll(center, swipeRadius, playerLayer)
            : Physics2D.OverlapCircleAll(center, swipeRadius);

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (col != null && col.CompareTag("Player"))
            {
                if (col.TryGetComponent(out HasHealth health))
                {
                    health.ChangeHealth(-swipeDamage);
                }
            }
        }
    }

    public void ShootSnowballAt(Vector2 target)
    {
        if (snowballPrefab == null)
        {
            Debug.LogWarning("Icyman: Snowball prefab not assigned.");
            return;
        }

        Vector2 spawnPos = snowballSpawn != null ? (Vector2)snowballSpawn.position : (Vector2)transform.position;
        GameObject snowball = Object.Instantiate(snowballPrefab, spawnPos, Quaternion.identity);
        Vector2 dir = (target - spawnPos).normalized;

        if (snowball.TryGetComponent(out Rigidbody2D rb2d))
        {
            rb2d.linearVelocity = dir * snowballSpeed;
        }
    }
}
