using UnityEngine;

public class CalculateState : State
{
    private readonly Rigidbody2D rb;

    public CalculateState(StateMachine machine, Rigidbody2D rb) : base(machine)
    {
        this.rb = rb;
    }

    public override void Enter()
    {
        var p = (PirateShip)machine.Enemy;
        p.FindPlayerIfNeeded();

        if (p.playerRb == null)
        {
            p.moveTarget = rb.position;
            p.shootDir = Vector2.left;
            return;
        }

        Camera cam = Camera.main;
        Vector2 camPos = cam ? (Vector2)cam.transform.position : Vector2.zero;

        // Convert to camera-space (IMPORTANT for runner games)
        Vector2 piratePos = rb.position;
        Vector2 playerPos = p.playerRb.position;
        Vector2 pirateCS = piratePos - camPos;
        Vector2 playerCS = playerPos - camPos;

        // Player velocity is already “per second” in world space.
        // In camera-space it’s the same *unless* you also want to subtract camera velocity.
        Vector2 playerVel = p.playerRb.linearVelocity;

        // 1) Aim direction computed in camera-space
        Vector2 shootDirCS = LeadAim2D.GetShootDirection(pirateCS, playerCS, playerVel, p.bulletSpeed);
        p.shootDir = shootDirCS; // direction is the same in world & cam space

        // 2) Choose a firing position in camera-space (range + strafe)
        Vector2 fireCS = playerCS - shootDirCS * p.desiredRange;

        Vector2 perp = new Vector2(-shootDirCS.y, shootDirCS.x);
        fireCS += perp * (p.strafeOffset * p.strafeSign);

        // 3) Clamp inside camera view (camera-space clamp via viewport)
        fireCS = ClampCamSpaceToViewport(fireCS, cam, 0.08f);

        // Convert back to world-space for movement
        p.moveTarget = fireCS + camPos;

        p.strafeSign *= -1;
    }

    private Vector2 ClampCamSpaceToViewport(Vector2 camSpacePos, Camera cam, float pad)
    {
        if (!cam) return camSpacePos;

        float z = 0f;
        float camDist = Mathf.Abs(cam.transform.position.z - z);

        // Get bounds in WORLD, then convert those bounds into CAMERA SPACE by subtracting cam position.
        Vector2 camPos = cam.transform.position;

        float xMinW = cam.ViewportToWorldPoint(new Vector3(pad, 0.5f, camDist)).x;
        float xMaxW = cam.ViewportToWorldPoint(new Vector3(1f - pad, 0.5f, camDist)).x;

        float yMinW = cam.ViewportToWorldPoint(new Vector3(0.5f, pad, camDist)).y;
        float yMaxW = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f - pad, camDist)).y;

        float xMin = xMinW - camPos.x;
        float xMax = xMaxW - camPos.x;
        float yMin = yMinW - camPos.y;
        float yMax = yMaxW - camPos.y;

        camSpacePos.x = Mathf.Clamp(camSpacePos.x, xMin, xMax);
        camSpacePos.y = Mathf.Clamp(camSpacePos.y, yMin, yMax);

        return camSpacePos;
    }


    public override void Exit() { }

    public override void Update()
    {
        machine.ChangeState(new MoveState(machine, rb));
    }
}
