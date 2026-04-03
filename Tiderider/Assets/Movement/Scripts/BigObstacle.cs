// BigObstacle.cs

using UnityEngine;

/// <summary>
/// A larger variant of Obstacle. Sets its scale to a bigger size.
/// </summary>
/// <remarks>
/// Maintained by: Obstacle System
/// </remarks>
public class BigObstacle : Obstacle
{
    [SerializeField] private Vector3 bigSize = new Vector3(2f, 2f, 1f); // Adjust as needed

    /// <summary>
    /// Sets the scale of the big obstacle and runs base obstacle initialization.
    /// </summary>
    protected override void Start()
    {
        base.Start(); // Ensure Obstacle's Start() runs
        //transform.localScale = bigSize;
    }
}

