using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using DG.Tweening;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public static int CurrentPlayingLevelIndex;

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

        if (data.levelIndex <= highest)
        {
            MoveShipToIsland(data);
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
            CurrentPlayingLevelIndex = data.levelIndex;
            SceneObjectSpawner.sceneJsonFile = data.levelJson;
            SceneManager.LoadScene("Movement");
        });
    }

    private float FindIslandPosition(int index)
    {
        return levelObjects[index].transform.position.y;
    }
}
