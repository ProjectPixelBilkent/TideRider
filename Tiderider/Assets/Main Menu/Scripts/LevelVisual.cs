using UnityEngine;
using UnityEngine.UI;

public class LevelVisual : MonoBehaviour
{
    public LevelData myData;
    public Color lockedColor = new Color(0.2f, 0.2f, 0.2f);

    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        int highest = DataManager.GetHighestUnlockedIndex();

        image.sprite = myData.islandSprite;
        image.color = (myData.levelIndex <= highest) ? myData.activeColor : lockedColor;
    }
}