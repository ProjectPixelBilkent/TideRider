// BigObstacle.cs

using UnityEngine;

public class BigObstacle : Obstacle
{
    /// <summary>
    /// A larger variant of Obstacle. Sets its scale to a bigger size.
    /// </summary>
    /// <remarks>
    /// Maintained by: Obstacle System
    /// </remarks>
    [SerializeField] private Vector3 bigSize = new Vector3(2f, 2f, 1f); // Adjust as needed


    /// <summary>
    /// Sets the scale of the big obstacle.
    /// </summary>
    void Start()
    {
        transform.localScale = bigSize;
    }
}
