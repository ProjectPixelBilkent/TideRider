using UnityEngine;
using UnityEngine.UI;

public class HasHealth: MonoBehaviour   
{
    public int maxHealth;
    public int currentHealth;
    protected Collider2D coll2d;
    public Slider healthSlider;
    


    protected virtual void Start()
    {
        Restore();
       
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        BulletProjectile mermi = collision.gameObject.GetComponent<BulletProjectile>();
        if (mermi != null)
        {
            int cLevel = mermi.level;

            int damage = mermi.cannon.weaponLevels[cLevel].damage;
            ChangeHealth(damage);
            Destroy(collision.gameObject);
        }
    }
    public void Update()
    {
       healthSlider.value = currentHealth;
        
    }
    public void Restore()
    {
        currentHealth = maxHealth;
        coll2d = GetComponent<Collider2D>();
    }

    public int ChangeHealth(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        if(currentHealth<=0)
        {
            Die();
        }
        return currentHealth;
        //Debug.Log("shot");
    }

    public virtual void Die()
    {
        if(currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
