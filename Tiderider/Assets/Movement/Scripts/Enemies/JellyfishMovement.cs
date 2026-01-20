using UnityEngine;

public class JellyfishMovement : MonoBehaviour
{
    public Transform ship;

    [Header("Follow Feel (normal)")]
    public float acceleration = 12f;   // how quickly it speeds up to catch ship
    public float maxXSpeed = 4f;       // top horizontal speed (ship can outrun it)
    public float damping = 8f;         // how quickly it slows when near target

    [Header("Y / Float")]
    public bool keepInitialY = true;
    public float fixedY = 0f;
    public float wobbleY = 0.12f;
    public float wobbleSpeed = 2f;

    float startY;
    float xVel; // current jellyfish x velocity

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        if (ship == null) return;

        float targetX = ship.position.x;
        float dx = targetX - transform.position.x;

        // Accelerate toward ship X (springy feel)
        float desiredAccel = dx * acceleration;

        // Apply acceleration
        xVel += desiredAccel * Time.deltaTime;

        // Damping so it doesn't jitter or overshoot forever
        xVel = Mathf.Lerp(xVel, 0f, damping * Time.deltaTime);

        // Clamp speed so ship can outrun it
        xVel = Mathf.Clamp(xVel, -maxXSpeed, maxXSpeed);

        // Move
        Vector3 pos = transform.position;
        pos.x += xVel * Time.deltaTime;

        // Y stays independent + wobble
        float y = keepInitialY ? startY : fixedY;
        y += Mathf.Sin(Time.time * wobbleSpeed) * wobbleY;
        pos.y = y;

        transform.position = pos;
    }
}
