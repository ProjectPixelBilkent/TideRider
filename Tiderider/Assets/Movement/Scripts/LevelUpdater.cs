using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUpdater : MonoBehaviour
{
    private const int FinalGameplayLevelIndex = 8;
    private const string BossFightSceneName = "BossFight";
    private const string FinalLevelBossIntroConversationId = "end_of_world_3_boss_fight";

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
        if (LevelManager.CurrentPlayingLevelIndex == FinalGameplayLevelIndex)
        {
            SceneObjectSpawner spawner = FindFirstObjectByType<SceneObjectSpawner>();
            if (spawner != null)
            {
                spawner.PlayRuntimeDialogue(FinalLevelBossIntroConversationId, LoadBossFightScene, false);
                return;
            }

            LoadBossFightScene();
            return;
        }

        if (!IntroLevelLoader.IsPlayingIntro)
        {
            DataManager.CompleteLevel(LevelManager.CurrentPlayingLevelIndex);
        }
        IntroLevelLoader.IsPlayingIntro = false;
        SceneManager.LoadScene("MainMenu");
    }

    private void LoadBossFightScene()
    {
        SceneManager.LoadScene(BossFightSceneName);
    }
}
