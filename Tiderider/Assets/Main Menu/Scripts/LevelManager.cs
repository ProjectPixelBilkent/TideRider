using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DG.Tweening;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public static int CurrentPlayingLevelIndex;
    private const int TotalLevelCount = 3;

    [Header("LevelObjects")]
    [SerializeField] private List<GameObject> levelObjects = new List<GameObject>();

    [Header("Ship Settings")]
    public Transform ship;
    public float travelSpeed = 1.5f;

    private void Awake() => Instance = this;

    private void Start()
    {
        int highest = DataManager.GetHighestUnlockedIndex();
        if (highest < levelObjects.Count)
        {
            ship.position = new Vector3(ship.position.x, levelObjects[highest].transform.position.y, ship.position.z);
        }
    }

    public void LoadLevel(LevelData data)
    {
        int highest = DataManager.GetHighestUnlockedIndex();

        if (data.levelIndex < highest)
        {
            MoveShipToIsland(data);
        }
        else
        {
            StartGameplay(data);
        }
    }

    private void MoveShipToIsland(LevelData data)
    {
        float targetY = FindIslandPosition(data.levelIndex);
        Vector3 targetPos = new Vector3(ship.position.x, targetY, ship.position.z);

        Sequence moveSequence = DOTween.Sequence();

        moveSequence.Append(ship.DOLocalRotate(new Vector3(0, 0, 180), 0.3f));

        moveSequence.Append(ship.DOMove(targetPos, travelSpeed));

        moveSequence.Append(ship.DOLocalRotate(Vector3.zero, 0.3f));

        moveSequence.OnComplete(() =>
        {
            StartGameplay(data);
        });
    }

    private float FindIslandPosition(int index)
    {
        return levelObjects[index].transform.position.y;
    }

    private void StartGameplay(LevelData data)
    {
        if (!ArmoryManager.Instance.isArmoryComplete())
        {
            NotificationManager.Instance.ShowNotification("Complete your armory!", NotificationManager.UITab.Armory);
            return;
        }

        if (ResourceManager.isEnergyLeft())
        {
            DataManager.DecrementEnergyAmount();
            CurrentPlayingLevelIndex = data.levelIndex;
            SceneObjectSpawner.sceneJsonFile = data.levelJson;
            SceneManager.LoadScene("Movement");
        }
        else
        {
            ResourceManager.HandleNoEnergy();
        }
    }

    //private void UnlockAllLevelsForDebug()
    //{
    //    int highestUnlocked = DataManager.GetHighestUnlockedIndex();

    //    while (highestUnlocked < TotalLevelCount)
    //    {
    //        DataManager.CompleteLevel(highestUnlocked);
    //        highestUnlocked = DataManager.GetHighestUnlockedIndex();
    //    }

    //    if (highestUnlocked > 0 && highestUnlocked - 1 < levelObjects.Count)
    //    {
    //        ship.position = new Vector3(ship.position.x, levelObjects[highestUnlocked - 1].transform.position.y, ship.position.z);
    //    }

    //    Debug.Log("Debug unlock applied up to level 3.");
    //}
}
