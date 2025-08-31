using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Up, Down, Right, Left
}

public class ExternalEffect : MonoBehaviour
{
    private static Dictionary<Direction, Vector3> directionVectors = new Dictionary<Direction, Vector3>
    {
        { Direction.Left, Vector3.left },
        { Direction.Right, Vector3.right },
        { Direction.Up, Vector3.up },
        { Direction.Down, Vector3.down }
    };

    [SerializeField] private Direction direction;
    [SerializeField] private float speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetAddition(GameObject ship)
    {
        return directionVectors[direction] * speed;
    }
}
