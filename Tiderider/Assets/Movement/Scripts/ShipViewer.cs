using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles UI display for ship health and speed.
/// </summary>
/// <remarks>
/// Maintained by: UI System
/// </remarks>
public class ShipViewer : MonoBehaviour
{
    [SerializeField] ShipModel model;
    [SerializeField] ShipController controller;
    [SerializeField] Slider healthSlider;
    [SerializeField] Slider speedSlider;
    [SerializeField] private float SpeedBarPercent = 20f;

    /// <summary>
    /// Subscribes to health change events and updates UI.
    /// </summary>
    private void Start()
    {
        if (model != null)
        {
            model.HealthChanged += OnHealthChanged;
        }
        UpdateView();
    }

    /// <summary>
    /// Unsubscribes from health change events.
    /// </summary>
    private void OnDestroy()
    {
        if (model != null)
        {
            model.HealthChanged -= OnHealthChanged;
        }
    }

    /// <summary>
    /// Damages the ship by the specified amount.
    /// </summary>
    /// <param name="amount">Amount of damage.</param>
    public void Damage(int amount)
    {
        model?.Decrement(amount);
    }

    /// <summary>
    /// Heals the ship by the specified amount.
    /// </summary>
    /// <param name="amount">Amount to heal.</param>
    public void Heal(int amount)
    {
        model?.Increment(amount);
    }

    /// <summary>
    /// Restores the ship's health to maximum.
    /// </summary>
    public void Reset()
    {
        model?.Restore();
    }

    /// <summary>
    /// Updates the health and speed sliders.
    /// </summary>
    public void UpdateView()
    {
        if (model == null)
            return;

        if (healthSlider != null && model.MaxHealth != 0)
        {
            healthSlider.value = (float)model.CurrentHealth / (float)model.MaxHealth;
        }

        if (speedSlider != null && controller != null && controller.maxVelocity > 0f)
        {
            speedSlider.value = controller.currentVelocity.magnitude / controller.maxVelocity;
        }
    }

    /// <summary>
    /// Called when the ship's health changes.
    /// </summary>
    public void OnHealthChanged()
    {
        UpdateView();
    }

    /// <summary>
    /// Updates the speed slider every frame.
    /// </summary>
    private void Update()
    {
        if (speedSlider != null && controller != null && controller.maxVelocity > 0f)
        {
            speedSlider.value = (controller.currentVelocity.magnitude / controller.maxVelocity) + SpeedBarPercent / 100;
        }
    }
}

