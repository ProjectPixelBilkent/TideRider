using UnityEngine;
using UnityEngine.UI;
using System;

public class HasHealth: MonoBehaviour   
{
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int currentHealth;
    protected Collider2D coll2d;
    public Slider healthSlider;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    public event Action HealthChanged;



    protected virtual void Start()
    {
        Restore();
       
    }
    public void Update()
    {
        if(healthSlider!=null)
        {
            healthSlider.value = currentHealth;
        }
    }
    public void Restore()
    {
        currentHealth = maxHealth;
        coll2d = GetComponent<Collider2D>();
    }

    public void SetMaxHealth(int value, bool restoreToFull = true)
    {
        maxHealth = Mathf.Max(1, value);
        currentHealth = restoreToFull ? maxHealth : Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public int TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return currentHealth;
        }

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        HealthChanged?.Invoke();
        if (currentHealth <= 0)
        {
            Die();
        }

        return currentHealth;
    }

    public int Heal(int amount)
    {
        if (amount <= 0)
        {
            return currentHealth;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        HealthChanged?.Invoke();
        return currentHealth;
    }

    public virtual void Die()
    {
        if(currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
