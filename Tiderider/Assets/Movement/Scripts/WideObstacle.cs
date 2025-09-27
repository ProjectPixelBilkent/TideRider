using UnityEngine;

public class WideObstacle : BigObstacle
{
    [SerializeField] private float[] possibleXPositions;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        if(possibleXPositions.Length > 0)
        {
            transform.position = new Vector3(possibleXPositions[Random.Range(0, possibleXPositions.Length)], transform.position.y, transform.position.z);
        }
    }


    protected override void UpdateColliderToMatchSprite(Sprite sprite) { }
}
