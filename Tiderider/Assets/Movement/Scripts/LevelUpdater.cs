using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUpdater : MonoBehaviour
{
    private void OnEnable()
    {
        EndingObject.OnLevelCompleted += HandleLevelFinished;
    }

    private void OnDisable()
    {
        EndingObject.OnLevelCompleted -= HandleLevelFinished;
    }

    private void HandleLevelFinished()
    {
        DataManager.CompleteLevel(LevelManager.CurrentPlayingLevelIndex);

        SceneManager.LoadScene("MainMenu");
    }
}