using Unity.VisualScripting;
using UnityEngine;

public class HasHealth: MonoBehaviour   
{
    private int maxHealth;
    private int currentHealth;
    private Collider2D coll2d;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    public int ChangeHealth(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        if(currentHealth<=0)
        {
            Die();
        }
        return currentHealth;
    }

    public void Die()
    {
        //TODO
    }
}
