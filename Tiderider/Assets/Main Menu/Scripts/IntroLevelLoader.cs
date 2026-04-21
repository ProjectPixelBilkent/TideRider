using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Checks on first game launch whether the player has seen the intro level.
/// If not, loads it immediately and marks it as seen so it never repeats.
/// Assign the level_intro JSON TextAsset in the Inspector.
/// </summary>
public class IntroLevelLoader : MonoBehaviour
{
    private const string MovementSceneName = "Movement";

    public static bool IsPlayingIntro = false;

    [SerializeField] private TextAsset introLevelJson;

    private void Start()
    {
        if (introLevelJson == null)
        {
            Debug.LogWarning("IntroLevelLoader: introLevelJson is not assigned.");
            return;
        }

        if (!DataManager.GetHasSeenIntro())
        {
            DataManager.SetHasSeenIntro();
            IsPlayingIntro = true;
            LevelManager.CurrentPlayingLevelIndex = 0;
            SceneObjectSpawner.sceneJsonFile = introLevelJson;
            SceneManager.LoadScene(MovementSceneName);
        }
    }
}
