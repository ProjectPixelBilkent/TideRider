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
    [SerializeField] Player player;
    [SerializeField] PlayerMovement controller;
    [SerializeField] Slider healthSlider;
    [SerializeField] Slider speedSlider;
    [SerializeField] private float SpeedBarPercent = 0f;

    /// <summary>
    /// Subscribes to health change events and updates UI.
    /// </summary>
    private void Start()
    {
        if (player != null)
            player.HealthChanged += OnHealthChanged;

        UpdateView();
    }

    private void OnDestroy()
    {
        if (player != null)
            player.HealthChanged -= OnHealthChanged;
    }

    /// <summary>
    /// Updates the health and speed sliders.
    /// </summary>
    public void UpdateView()
    {
        if (player == null)
            return;

        if (healthSlider != null && player.MaxHealth != 0)
        {
            healthSlider.value = (float)player.CurrentHealth / (float)player.MaxHealth;
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

