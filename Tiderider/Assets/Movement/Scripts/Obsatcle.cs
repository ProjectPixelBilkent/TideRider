using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private float speed = 20f; // Match your ship's forwardSpeed
    [SerializeField] private float destroyY = -15f; // Destroy when off-screen
    
    void Update()
    {
        // Move downward
        transform.position += Vector3.down * speed * Time.deltaTime;
        
        // Destroy when off-screen
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}