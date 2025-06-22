using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private float speed = 20f; // Match your ship's forwardSpeed
    
    void Update()
    {
        // Move downward
        transform.position += Vector3.down * speed * Time.deltaTime;
    }
    
    // Automatically called when object goes off-screen
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}