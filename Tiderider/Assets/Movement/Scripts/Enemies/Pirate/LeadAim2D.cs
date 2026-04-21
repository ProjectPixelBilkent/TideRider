using UnityEngine;

public static class LeadAim2D
{
    // Returns a normalized direction to shoot so a bullet (speed = bulletSpeed) can intercept the player.
    // If no valid intercept exists, it returns direct aim at the player.
    public static Vector2 GetShootDirection(Vector2 piratePos, Vector2 playerPos, Vector2 playerVel, float bulletSpeed)
    {
        Vector2 direct = playerPos - piratePos;
        if (direct.sqrMagnitude < 0.0001f) return Vector2.right;
        Vector2 fallback = direct.normalized;

        if (bulletSpeed <= 0.01f) return fallback;

        Vector2 r = playerPos - piratePos;

        // Solve |r + v*t|^2 = (s*t)^2
        float a = Vector2.Dot(playerVel, playerVel) - bulletSpeed * bulletSpeed;
        float b = 2f * Vector2.Dot(r, playerVel);
        float c = Vector2.Dot(r, r);

        float t;

        if (Mathf.Abs(a) < 1e-6f)
        {
            if (Mathf.Abs(b) < 1e-6f) return fallback;
            t = -c / b;
            if (t <= 0f) return fallback;
        }
        else
        {
            float disc = b * b - 4f * a * c;
            if (disc < 0f) return fallback;

            float sqrtDisc = Mathf.Sqrt(disc);
            float t1 = (-b - sqrtDisc) / (2f * a);
            float t2 = (-b + sqrtDisc) / (2f * a);

            // Smallest positive time
            t = (t1 > 0f && t2 > 0f) ? Mathf.Min(t1, t2) : Mathf.Max(t1, t2);
            if (t <= 0f) return fallback;
        }

        Vector2 aimPoint = playerPos + playerVel * t;
        Vector2 dir = aimPoint - piratePos;

        if (dir.sqrMagnitude < 0.0001f) return fallback;
        return dir.normalized;
    }
}
