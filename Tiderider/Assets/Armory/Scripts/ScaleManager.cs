using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MonoBehaviour class for scaling the elements of scenes.
/// </summary>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
public class ScaleManager : MonoBehaviour
{
    public Camera Camera;
    private const int ORIGINAL_WIDTH = 1080, ORIGINAL_HEIGHT = 2400, TOP_MENU_HEIGHT = 200, TOP_MENU_PADDING = 25, TOP_MENU_GAP = 25, GEAR_ICON_SIZE = 100, ICON_MENU_HEIGHT = 200;
    public static float Width, Height, FrameWidth, FrameHeight, SelectedIconWidth, SideIconWidth;
    public static Color SelectedColor, SideColor;
    [SerializeField] private RectTransform topMenu, navigationMenu;

    [Header("Armory References")]
    public RectTransform armoryCanvas;
    public RectTransform armoryTopCanvas, armoryBottomCanvas, Ship, weaponsPanel, firstRow, secondRow;
    public RectTransform[] shipSlots, weaponSlots;
    private const int SHIP_SLOT_COUNT = 3, SHIP_SLOT_OFFSET = 25, SHIP_SLOT_GAP = 100, ARMORY_ROW_GAP = 40, ARMORY_SLOT_GAP = 15, WEAPONS_PANEL_MARGIN = 40;
    private const float SHIP_RATIO = 0.73f;

    [Header("Level Menu References")]
    public RectTransform levelCanvas;

    [Header("Shop References")]
    public RectTransform shopCanvas;

    private void Start()
    {
        Width = Camera.pixelWidth;
        Height = Camera.pixelHeight;

        ScaleTopMenu();
        ScaleArmory();
        ScaleLevels();
        ScaleShop();
        ScaleNavigationMenu();
    }

    /// <summary>
    /// Scales the top menu elements based on the camera's pixel dimensions.
    /// </summary>
    /// <remarks>
    /// Created by: Işık Dönger
    /// </remarks>
    private void ScaleTopMenu()
    {
        int height = Camera.pixelHeight, width = Camera.pixelWidth;
        float scaleFactor = width / (float)ORIGINAL_WIDTH;
        GameObject resourcePanel = topMenu.transform.GetChild(1).gameObject;
        RectTransform accountFrameRect = topMenu.transform.GetChild(0).GetComponent<RectTransform>(), resourcePanelRect = resourcePanel.GetComponent<RectTransform>();
        RectTransform energyFrameRect = resourcePanelRect.transform.GetChild(0).GetComponent<RectTransform>(),
            moneyFrameRect = resourcePanelRect.transform.GetChild(1).GetComponent<RectTransform>();

        topMenu.sizeDelta = new Vector2(width, TOP_MENU_HEIGHT);
        topMenu.anchoredPosition = new Vector2(0, -TOP_MENU_HEIGHT / 2);
        accountFrameRect.sizeDelta = new Vector2(accountFrameRect.sizeDelta.x * scaleFactor, TOP_MENU_HEIGHT);
        accountFrameRect.anchoredPosition = new Vector2(accountFrameRect.sizeDelta.x / 2, 0);
        resourcePanelRect.sizeDelta = new Vector2(resourcePanelRect.sizeDelta.x * scaleFactor, resourcePanelRect.sizeDelta.y);
        resourcePanelRect.anchoredPosition = new Vector2(-resourcePanelRect.sizeDelta.x / 2 - TOP_MENU_PADDING, 0);
        energyFrameRect.sizeDelta = new Vector2((resourcePanelRect.sizeDelta.x - GEAR_ICON_SIZE - TOP_MENU_GAP * 2) / 2, energyFrameRect.sizeDelta.y);
        energyFrameRect.anchoredPosition = new Vector2(energyFrameRect.sizeDelta.x / 2, 0);
        moneyFrameRect.sizeDelta = new Vector2((resourcePanelRect.sizeDelta.x - GEAR_ICON_SIZE - TOP_MENU_GAP * 2) / 2, moneyFrameRect.sizeDelta.y);
        moneyFrameRect.anchoredPosition = new Vector2(moneyFrameRect.sizeDelta.x * 1.5f + TOP_MENU_GAP, 0); // Place money frame next to energy frame with menu gap
    }

    /// <summary>
    /// Scales the armory UI elements based on the camera's pixel dimensions.
    /// </summary>
    /// <remarks>
    /// Created by: Işık Dönger
    /// </remarks>
    private void ScaleArmory()
    {
        armoryCanvas.sizeDelta = new Vector2(Width, Height);
        armoryCanvas.anchoredPosition = new Vector2(-Width, 0);
        armoryTopCanvas.sizeDelta = new Vector2(0, Width);
        armoryTopCanvas.anchoredPosition = new Vector2(0, -Width / 2);
        armoryBottomCanvas.sizeDelta = new Vector2(0, Width);
        armoryBottomCanvas.anchoredPosition = new Vector2(0, Width / 2);
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

        firstRow.sizeDelta = new Vector2(0, (armoryBottomCanvas.rect.height - ICON_MENU_HEIGHT - WEAPONS_PANEL_MARGIN - ARMORY_ROW_GAP) / 2);
        firstRow.anchoredPosition = new Vector2(0, -firstRow.sizeDelta.y / 2);
        secondRow.sizeDelta = new Vector2(0, (armoryBottomCanvas.rect.height- ICON_MENU_HEIGHT - WEAPONS_PANEL_MARGIN - ARMORY_ROW_GAP) / 2);
        secondRow.anchoredPosition = new Vector2(0, secondRow.sizeDelta.y / 2 + WEAPONS_PANEL_MARGIN);

        foreach (RectTransform slot in weaponSlots)
        {
            slot.sizeDelta = new Vector2((Width - ARMORY_SLOT_GAP * 2) / 3, 0);

            if (slot.transform.GetSiblingIndex() == 0)
            {
                slot.anchoredPosition = new Vector2(slot.rect.width / 2, slot.anchoredPosition.y);
            }
            else if (slot.transform.GetSiblingIndex() == 2)
            {
                slot.anchoredPosition = new Vector2(-slot.rect.width / 2, slot.anchoredPosition.y);
            }
        }

        FrameWidth = weaponSlots[0].rect.width;
        FrameHeight = weaponSlots[0].rect.height;
    }

    /// <summary>
    /// Scales the level UI elements based on the camera's pixel dimensions.
    /// </summary>
    /// <remarks>
    /// Created by: Işık Dönger
    /// </remarks>
    private void ScaleLevels()
    {
        levelCanvas.sizeDelta = new Vector2(Width, Height);
        levelCanvas.anchoredPosition = new Vector2(0, 0);
    }

    /// <summary>
    /// Scales the shop UI elements based on the camera's pixel dimensions.
    /// </summary>
    /// <remarks>
    /// Created by: Işık Dönger
    /// </remarks>
    private void ScaleShop()
    {
        shopCanvas.sizeDelta = new Vector2(Width, Height);
        shopCanvas.anchoredPosition = new Vector2(Width, 0);
    }

    /// <summary>
    /// Scales the navigation menu elements based on the camera's pixel dimensions.
    /// </summary>
    /// <remarks>
    /// Created by: Işık Dönger
    /// </remarks>
    private void ScaleNavigationMenu()
    {
        int height = Camera.pixelHeight, width = Camera.pixelWidth;
        float scaleFactor = width / (float)ORIGINAL_WIDTH;
        RectTransform armoryIconRect = navigationMenu.transform.GetChild(0).GetComponent<RectTransform>(),
            mainMenuIconRect = navigationMenu.transform.GetChild(1).GetComponent<RectTransform>(),
            shopIconRect = navigationMenu.transform.GetChild(2).GetComponent<RectTransform>();

        navigationMenu.sizeDelta = new Vector2(width, ICON_MENU_HEIGHT);
        navigationMenu.anchoredPosition = new Vector2(0, ICON_MENU_HEIGHT / 2);
        armoryIconRect.sizeDelta = new Vector2(armoryIconRect.sizeDelta.x * scaleFactor, ICON_MENU_HEIGHT);
        armoryIconRect.anchoredPosition = new Vector2(armoryIconRect.sizeDelta.x / 2, 0);
        SideIconWidth = armoryIconRect.sizeDelta.x;
        SideColor = armoryIconRect.GetComponent<Image>().color;
        mainMenuIconRect.sizeDelta = new Vector2(mainMenuIconRect.sizeDelta.x * scaleFactor, ICON_MENU_HEIGHT);
        mainMenuIconRect.anchoredPosition = new Vector2(0, 0);
        SelectedIconWidth = mainMenuIconRect.sizeDelta.x;
        SelectedColor = mainMenuIconRect.GetComponent<Image>().color;
        shopIconRect.sizeDelta = new Vector2(shopIconRect.sizeDelta.x * scaleFactor, ICON_MENU_HEIGHT);
        shopIconRect.anchoredPosition = new Vector2(-shopIconRect.sizeDelta.x / 2, 0);
    }
}
