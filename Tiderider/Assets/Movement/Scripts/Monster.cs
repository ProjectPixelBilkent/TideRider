using UnityEngine;

public class Monster : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        print("Monster collided with " + collision.gameObject.name);
        HandlePlayerContact(collision.gameObject);
    }

    private void HandlePlayerContact(GameObject target)
    {
        Player player = target.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(player.MaxHealth);
        }
    }
}
