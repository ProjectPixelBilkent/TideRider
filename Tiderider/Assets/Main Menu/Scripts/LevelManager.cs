using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public static int CurrentPlayingLevelIndex = -1;

    private bool canInteract = false;

    [Header("UI Map References")]
    [SerializeField] private RectTransform mapContentContainer;
    [SerializeField] private float bottomScreenPadding = 300f;

    [Header("LevelObjects")]
    [SerializeField] private List<GameObject> levelObjects = new List<GameObject>();

    [Header("Ship Settings")]
    public Transform ship;
    private ShipNavigator shipNavigator;

    private void Awake()
    {
        Instance = this;

        shipNavigator = ship.GetComponent<ShipNavigator>();

        if (shipNavigator == null)
        {
            shipNavigator = ship.gameObject.AddComponent<ShipNavigator>();
        }
    }

    private void Start()
    {
        InitializeMapPosition();
        StartCoroutine(EnableInteractionAfterDelay());
    }

    private IEnumerator EnableInteractionAfterDelay()
    {
        // Skip two frames so any leftover pointer events from the previous scene
        // (phantom clicks) are consumed before we allow island interactions.
        yield return null;
        yield return null;
        canInteract = true;
    }

    private void InitializeMapPosition()
    {
        if (levelObjects == null || levelObjects.Count == 0) return;

        int targetIndex = CurrentPlayingLevelIndex != -1
            ? CurrentPlayingLevelIndex
            : DataManager.GetHighestUnlockedIndex();

        targetIndex = Mathf.Clamp(targetIndex, 0, levelObjects.Count - 1);

        GameObject targetLevelObj = levelObjects[targetIndex];

        shipNavigator.SnapToLevel(targetLevelObj);

        if (mapContentContainer != null)
        {
            RectTransform levelRect = targetLevelObj.GetComponent<RectTransform>();

            float newYPosition = -levelRect.anchoredPosition.y + bottomScreenPadding;

            mapContentContainer.anchoredPosition = new Vector2(
                mapContentContainer.anchoredPosition.x,
                newYPosition
            );
        }
    }

    public void LoadLevel(LevelData data)
    {
        if (!canInteract) return;

#if UNITY_EDITOR
        Debug.Log("Loading level: " + data.name);
#endif
        if (!ArmoryManager.Instance.isArmoryComplete())
        {
            NotificationManager.Instance.ShowNotification("Complete your armory!", NotificationManager.UITab.Armory);
            return;
        }

        int highestUnlocked = DataManager.GetHighestUnlockedIndex();
        if (data.levelIndex > highestUnlocked)
        {
            NotificationManager.Instance?.ShowNotification("This island is locked.");
            return;
        }

        if (!ResourceManager.isEnergyLeft())
        {
            ResourceManager.HandleNoEnergy();
            return;
        }

        DataManager.DecrementEnergyAmount();

        shipNavigator.NavigateTo(
            levelObjects[data.levelIndex],
            () => StartGameplay(data)
        );
    }

    private void StartGameplay(LevelData data)
    {
        CurrentPlayingLevelIndex = data.levelIndex;
        SceneObjectSpawner.sceneJsonFile = data.levelJson;
        SceneManager.LoadScene("Movement");
    }
}