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
    private const float shipRatio = 0.65f, shipSlotOffset = 25f, shipSlotGap = 100f, armoryRowGap = 80f, armorySlotGap = 15f;

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
        Ship.sizeDelta = new Vector2(Ship.rect.height * shipRatio, 0);

        foreach (RectTransform slot in shipSlots)
        {
            float totalOffset = shipSlotOffset * 2 + shipSlotGap * 2;
            slot.sizeDelta = new Vector2((Ship.rect.height - totalOffset) / 3, (Ship.rect.height - totalOffset) / 3);

            if (slot.transform.GetSiblingIndex() < 2)
            {
                slot.anchoredPosition = new Vector2(slot.anchoredPosition.x, -slot.rect.height / 2 - shipSlotOffset);
            }
            else if (slot.transform.GetSiblingIndex() > 3)
            {
                slot.anchoredPosition = new Vector2(slot.anchoredPosition.x, slot.rect.height / 2 + shipSlotOffset);
            }
        }

        firstRow.sizeDelta = new Vector2(0, (bottomCanvas.rect.height - armoryRowGap) / 2);
        firstRow.anchoredPosition = new Vector2(0, -firstRow.sizeDelta.y / 2);
        secondRow.sizeDelta = new Vector2(0, (bottomCanvas.rect.height - armoryRowGap) / 2);
        secondRow.anchoredPosition = new Vector2(0, firstRow.sizeDelta.y / 2);

        foreach (RectTransform slot in weaponSlots)
        {
            slot.sizeDelta = new Vector2((width - armorySlotGap * 2) / 3, 0);

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
