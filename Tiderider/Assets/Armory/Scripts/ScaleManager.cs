using UnityEngine;

/// <summary>
/// MonoBehaviour class for scaling the elements of scenes.
/// </summary>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
public class ScaleManager : MonoBehaviour
{
    public Camera Camera;
    public RectTransform topCanvas;
    public RectTransform bottomCanvas;
    public RectTransform Ship;
    public RectTransform[] shipSlots;
    public RectTransform firstRow, secondRow;
    public RectTransform[] weaponSlots;
    public static float frameWidth, frameHeight;

    private void Start()
    {
        ScaleArmory();
    }

    /// <summary>
    /// Scales the armory UI elements based on the camera's pixel dimensions.
    /// </summary>
    /// <remarks>
    /// Created by: Işık Dönger
    /// </remarks>
    private void ScaleArmory()
    {
        int height = Camera.pixelHeight, width = Camera.pixelWidth;

        topCanvas.sizeDelta = new Vector2(0, width);
        topCanvas.anchoredPosition = new Vector2(0, -width / 2);
        bottomCanvas.sizeDelta = new Vector2(0, width);
        bottomCanvas.anchoredPosition = new Vector2(0, width / 2);
        Ship.sizeDelta = new Vector2(Ship.rect.height * 0.65f, 0);

        foreach (RectTransform slot in shipSlots)
        {
            slot.sizeDelta = new Vector2((Ship.rect.height - 250) / 3, (Ship.rect.height - 250) / 3);

            if (slot.transform.GetSiblingIndex() < 2)
            {
                slot.anchoredPosition = new Vector2(slot.anchoredPosition.x, -slot.rect.height / 2 - 25);
            }
            else if (slot.transform.GetSiblingIndex() > 3)
            {
                slot.anchoredPosition = new Vector2(slot.anchoredPosition.x, slot.rect.height / 2 + 25);
            }
        }

        firstRow.sizeDelta = new Vector2(0, (bottomCanvas.rect.height - 80) / 2);
        firstRow.anchoredPosition = new Vector2(0, -firstRow.sizeDelta.y / 2);
        secondRow.sizeDelta = new Vector2(0, (bottomCanvas.rect.height - 80) / 2);
        secondRow.anchoredPosition = new Vector2(0, firstRow.sizeDelta.y / 2);

        foreach (RectTransform slot in weaponSlots)
        {
            slot.sizeDelta = new Vector2((width - 30) / 3, 0);

            if (slot.transform.GetSiblingIndex() == 0)
            {
                slot.anchoredPosition = new Vector2(slot.rect.width / 2, slot.anchoredPosition.y);
            }
            else if (slot.transform.GetSiblingIndex() == 2)
            {
                slot.anchoredPosition = new Vector2(-slot.rect.width / 2, slot.anchoredPosition.y);
            }
        }

        frameWidth = weaponSlots[0].rect.width;
        frameHeight = weaponSlots[0].rect.height;
    }
}
