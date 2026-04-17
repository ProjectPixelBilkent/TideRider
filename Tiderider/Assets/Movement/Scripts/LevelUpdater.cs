using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUpdater : MonoBehaviour
{
    private const int FinalGameplayLevelIndex = 2;
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
                spawner.PlayRuntimeDialogue(FinalLevelBossIntroConversationId, LoadBossFightScene, true, false);
                return;
            }

            LoadBossFightScene();
            return;
        }

        DataManager.IncrementEnergyAmount();
        DataManager.CompleteLevel(LevelManager.CurrentPlayingLevelIndex);
        SceneManager.LoadScene("MainMenu");
    }

    private void LoadBossFightScene()
    {
        SceneManager.LoadScene(BossFightSceneName);
    }
}
