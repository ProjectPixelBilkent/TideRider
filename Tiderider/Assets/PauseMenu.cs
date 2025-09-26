using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the pause menu functionality. Pauses and resumes the game using UI buttons.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;

    private bool isPaused = false;

    /// <summary>
    /// Sets up button listeners.
    /// </summary>
    void Start()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(Pause);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(Resume);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    /// <summary>
    /// Pauses the game and shows the pause menu UI.
    /// </summary>
    public void Pause()
    {
        Debug.Log("Paused Game");
        Time.timeScale = 0f;
        isPaused = true;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);
    }

    /// <summary>
    /// Resumes the game and hides the pause menu UI.
    /// </summary>
    public void Resume()
    {
        Debug.Log("Resume Game");
        Time.timeScale = 1f;
        isPaused = false;
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }
}

