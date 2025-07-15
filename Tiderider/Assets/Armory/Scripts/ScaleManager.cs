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
    public RectTransform topCanvas, bottomCanvas, Ship, weaponsPanel, firstRow, secondRow;
    public RectTransform[] shipSlots, weaponSlots;
    private const int SHIP_SLOT_COUNT = 3, SHIP_SLOT_OFFSET = 25, SHIP_SLOT_GAP = 100, ICON_MENU_HEIGHT = 200, ICON_MENU_MARGIN = 40, ARMORY_ROW_GAP = 40, ARMORY_SLOT_GAP = 15;
    private const float SHIP_RATIO = 0.73f;
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
        Ship.sizeDelta = new Vector2(Ship.rect.height * SHIP_RATIO, 0);

        foreach (RectTransform slot in shipSlots)
        {
            float totalOffset = SHIP_SLOT_OFFSET * (SHIP_SLOT_COUNT - 1) + SHIP_SLOT_GAP * (SHIP_SLOT_COUNT - 1);
            float slotDimension = (Ship.rect.height - totalOffset) / SHIP_SLOT_COUNT;
            slot.sizeDelta = new Vector2(slotDimension, slotDimension);

            if (slot.transform.GetSiblingIndex() < 2)
            {
                slot.anchoredPosition = new Vector2(slot.anchoredPosition.x, -slot.rect.height / 2 - SHIP_SLOT_OFFSET);
            }
            else if (slot.transform.GetSiblingIndex() > 3)
            {
                slot.anchoredPosition = new Vector2(slot.anchoredPosition.x, slot.rect.height / 2 + SHIP_SLOT_OFFSET);
            }
        }

        firstRow.sizeDelta = new Vector2(0, (bottomCanvas.rect.height - ICON_MENU_HEIGHT - ICON_MENU_MARGIN - ARMORY_ROW_GAP) / 2);
        firstRow.anchoredPosition = new Vector2(0, -firstRow.sizeDelta.y / 2);
        secondRow.sizeDelta = new Vector2(0, (bottomCanvas.rect.height- ICON_MENU_HEIGHT - ICON_MENU_MARGIN - ARMORY_ROW_GAP) / 2);
        secondRow.anchoredPosition = new Vector2(0, secondRow.sizeDelta.y / 2 + ICON_MENU_MARGIN);

        foreach (RectTransform slot in weaponSlots)
        {
            slot.sizeDelta = new Vector2((width - ARMORY_SLOT_GAP * 2) / 3, 0);

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
