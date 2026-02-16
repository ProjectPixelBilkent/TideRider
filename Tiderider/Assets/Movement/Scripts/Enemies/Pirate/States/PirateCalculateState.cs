using UnityEngine;

public class PirateCalculateState : State
{
    private readonly Rigidbody2D rb;

    public PirateCalculateState(StateMachine machine, Rigidbody2D rb) : base(machine)
    {
        this.rb = rb;
    }

    public override void Enter()
    {
        var p = (PirateShip)machine.Enemy;
        p.FindPlayerIfNeeded();

        if (p.playerRb == null) return;

        // Get positions and velocities
        Vector2 piratePosW = rb.position;
        Vector2 playerPosW = p.playerRb.position;
        Vector2 playerVelW = p.playerRb.linearVelocity;

        // Work in camera space so the ship stays on-screen
        Camera cam = Camera.main;
        Vector2 camPos = cam ? (Vector2)cam.transform.position : Vector2.zero;
        Vector2 pirateCS = piratePosW - camPos;
        Vector2 playerCS = playerPosW - camPos;

        // STEP 1: Calculate lead-aim direction
        // This is where we need to shoot to hit the moving player
        // Uses bulletSpeed to predict intercept point
        Vector2 shootDirCS = LeadAim2D.GetShootDirection(
            pirateCS,
            playerCS,
            playerVelW,
            p.bulletSpeed
        );
        p.shootDir = shootDirCS;

        // STEP 2: Calculate optimal firing position
        // Position ourselves behind the shot line at desiredRange
        // This puts us at the right distance to shoot accurately
        Vector2 firePositionCS = playerCS - (shootDirCS * p.desiredRange);

        // STEP 3: Add follow component
        // Blend between firing position and following the player
        // This keeps us mobile and not too static
        float followWeight = 0.6f; // 60% follow, 40% firing position
        Vector2 targetCS = Vector2.Lerp(firePositionCS, playerCS, followWeight);

        // STEP 4: Add perpendicular strafing
        // Move side-to-side (perpendicular to shot direction)
        // This makes us harder to hit and gives varied attack angles
        Vector2 perpendicular = new Vector2(-shootDirCS.y, shootDirCS.x);
        targetCS += perpendicular * (p.strafeOffset * p.strafeSign);

        // Alternate strafe direction each recalculation
        p.strafeSign *= -1;

        // STEP 5: Clamp to viewport
        // Keep the ship on-screen so it doesn't fly off into space
        targetCS = ClampCamSpaceToViewport(targetCS, cam, 0.08f);

        // STEP 6: Convert back to world space
        p.moveTarget = targetCS + camPos;
        Debug.Log("PirateCalculateState set moveTarget to: " + p.moveTarget + " (player at " + playerPosW + ")");

    }

    private Vector2 ClampCamSpaceToViewport(Vector2 camSpacePos, Camera cam, float pad)
    {
        if (!cam) return camSpacePos;

        float z = 0f;
        float camDist = Mathf.Abs(cam.transform.position.z - z);
        Vector2 camPos = cam.transform.position;

        // Get viewport bounds in world space
        float xMinW = cam.ViewportToWorldPoint(new Vector3(pad, 0.5f, camDist)).x;
        float xMaxW = cam.ViewportToWorldPoint(new Vector3(1f - pad, 0.5f, camDist)).x;
        float yMinW = cam.ViewportToWorldPoint(new Vector3(0.5f, pad, camDist)).y;
        float yMaxW = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f - pad, camDist)).y;

        // Convert to camera space bounds
        float xMin = xMinW - camPos.x;
        float xMax = xMaxW - camPos.x;
        float yMin = yMinW - camPos.y;
        float yMax = yMaxW - camPos.y;

        // Clamp position
        camSpacePos.x = Mathf.Clamp(camSpacePos.x, xMin, xMax);
        camSpacePos.y = Mathf.Clamp(camSpacePos.y, yMin, yMax);

        return camSpacePos;
    }

    public override void Exit()
    {
    }

    public override void Update()
    {
        // Immediately transition to PirateMoveState (NOT generic MoveState!)
        machine.ChangeState(new PirateMoveState(machine, rb));
    }
}