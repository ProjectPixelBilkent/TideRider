using UnityEngine;

public class HasHealth: MonoBehaviour   
{
    public int maxHealth;
    public int currentHealth;
    protected Collider2D coll2d;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    protected virtual void Start()
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

    public virtual void Die()
    {
        //TODO
    }
}
