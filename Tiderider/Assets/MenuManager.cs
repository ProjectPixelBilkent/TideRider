using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages pause and game over menus. All UI elements are children of the Canvas running this script.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipModel shipModel;

    private GameObject pauseMenuUI;
    private GameObject gameOverMenuUI;
    private Button restartButtonPauseMenu;
    private Button restartButtonGameOverMenu;
    private Button resumeButtonPauseMenu;
    private Button pauseButton;
    private Button mainMenuButtonGameOverMenu;

    private bool isPaused = false;

    void Awake()
    {
        // Find UI elements among children (by name)
        pauseMenuUI = transform.Find("SafeArea/PauseMenu")?.gameObject;
        gameOverMenuUI = transform.Find("SafeArea/GameOverMenu")?.gameObject;
        restartButtonPauseMenu = transform.Find("SafeArea/PauseMenu/RestartButton")?.GetComponent<Button>();
        restartButtonGameOverMenu = transform.Find("SafeArea/GameOverMenu/RestartButton")?.GetComponent<Button>();
        resumeButtonPauseMenu = transform.Find("SafeArea/PauseMenu/ResumeButton")?.GetComponent<Button>();
        pauseButton = transform.Find("SafeArea/PauseButton")?.GetComponent<Button>();
        mainMenuButtonGameOverMenu = transform.Find("SafeArea/GameOverMenu/MainMenuButton")?.GetComponent<Button>();
    }

    void Start()
    {
        // Button listeners
        if (restartButtonPauseMenu != null)
            restartButtonPauseMenu.onClick.AddListener(RestartScene);

        if (restartButtonGameOverMenu != null)
            restartButtonGameOverMenu.onClick.AddListener(RestartScene);

        if (mainMenuButtonGameOverMenu != null)
            mainMenuButtonGameOverMenu.onClick.AddListener(GoToMainMenu);

        if (resumeButtonPauseMenu != null)
            resumeButtonPauseMenu.onClick.AddListener(Resume);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(Pause);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        if (gameOverMenuUI != null)
            gameOverMenuUI.SetActive(false);

        // Subscribe to health change
        if (shipModel != null)
            shipModel.HealthChanged += OnShipHealthChanged;
    }

    private void OnDestroy()
    {
        if (shipModel != null)
            shipModel.HealthChanged -= OnShipHealthChanged;
    }

    private void OnShipHealthChanged()
    {
        if (shipModel.CurrentHealth <= 0)
        {
            ShowGameOverMenu();
        }
    }

    public void Pause()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ShowGameOverMenu()
    {
        // Hide pause menu (and its Continue button) when showing the death screen
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        if (gameOverMenuUI != null)
            gameOverMenuUI.SetActive(true);

        isPaused = false;
        Time.timeScale = 0f;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    public void RestartScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
