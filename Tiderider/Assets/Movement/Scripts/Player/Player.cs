using UnityEngine;

public class Player : HasHealth
{
    [SerializeField] private PlayerMovement playerMovement;


    public override void Die()
    {
        if (currentHealth > 0) return;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (SoundLibrary.Instance != null)
            SoundLibrary.Instance.StopBGM();

        MenuManager menuManager = FindFirstObjectByType<MenuManager>();
        if (menuManager != null)
            menuManager.ShowGameOverMenu();
    }
}
