using Unity.VisualScripting;
using UnityEngine;

public class HasHealth: MonoBehaviour   
{
    public int maxHealth;
    public int currentHealth;
    public Collider2D coll2d;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    private void Start()
    {
        Restore();
    }

    public void Restore()
    {
        currentHealth = maxHealth;
        coll2d = GetComponent<Collider2D>();
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
