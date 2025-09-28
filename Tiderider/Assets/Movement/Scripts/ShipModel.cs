using UnityEngine;
using System;


public enum ShipModifier
{
    None,
    SlowedDown, 
    Burning, 
    Frozen
}


/// <summary>
/// Holds ship health data and provides methods to modify and restore health.
/// </summary>
/// <remarks>
/// Maintained by: Ship System
/// </remarks>
public class ShipModel : MonoBehaviour
{
    public event Action HealthChanged;

    private const int minHealth = 0;
    private const int maxHealth = 100;
    private int currentHealth;
    private ShipModifier modifier;

    public int CurrentHealth { get => currentHealth; set => currentHealth = value; }
    public int MinHealth => minHealth;
    public int MaxHealth => maxHealth;
    public ShipModifier Modifier => modifier;

    /// <summary>
    /// Increases ship health by the specified amount.
    /// </summary>
    /// <param name="amount">Amount to increase health.</param>
    public void Increment(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, minHealth, maxHealth);
        UpdateHealth();
    }

    /// <summary>
    /// Decreases ship health by the specified amount.
    /// </summary>
    /// <param name="amount">Amount to decrease health.</param>
    public void Decrement(int amount)
    {
                print(currentHealth);
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, minHealth, maxHealth);
        print(currentHealth);
        UpdateHealth();
    }

    /// <summary>
    /// Restores ship health to maximum.
    /// </summary>
    public void Restore()
    {
        currentHealth = maxHealth;
        UpdateHealth();
    }

    /// <summary>
    /// Invokes the HealthChanged event.
    /// </summary>
    public void UpdateHealth()
    {
        HealthChanged?.Invoke();
    }
}

