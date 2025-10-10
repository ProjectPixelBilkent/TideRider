using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MonoBehaviour class for scaling the elements of scenes.
/// </summary>
/// <todo>
/// Implement dynamic scaling for Profile, Setting and Credit panels.
/// </todo>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
public class ScaleManager : MonoBehaviour
{
    public Camera Camera;
    private const int ORIGINAL_WIDTH = 1080, ORIGINAL_HEIGHT = 2400, TOP_MENU_HEIGHT = 200, TOP_MENU_PADDING = 25, TOP_MENU_GAP = 25, GEAR_ICON_SIZE = 100, ICON_MENU_HEIGHT = 200;
    public static float Width, Height, FrameWidth, FrameHeight, SelectedIconWidth, SideIconWidth;
    public static Color SelectedColor, SideColor;

    [Header("Main Menu References")]
    [SerializeField] private RectTransform topMenu, navigationMenu;
    [SerializeField] private RectTransform menuCanvas, viewport, menuParent;

    [Header("Armory References")]
    [SerializeField] private RectTransform armoryCanvas;
    [SerializeField] private RectTransform armoryTopCanvas, armoryBottomCanvas, Ship, weaponsPanel, firstRow, secondRow;
    [SerializeField] private RectTransform[] shipSlots, weaponFrames;
    private const int SHIP_SLOT_COUNT = 3, SHIP_SLOT_OFFSET = 25, SHIP_SLOT_GAP = 100, ARMORY_ROW_GAP = 40, ARMORY_SLOT_GAP = 15, WEAPONS_PANEL_MARGIN = 40, WEAPONS_SLOT_PADDING = 25;
    private const float SHIP_RATIO = 0.73f, WEAPON_NAME_RATIO = 0.15f, UPGRADE_BTN_RATIO = 0.3f;

    [Header("Level Menu References")]
    [SerializeField] private RectTransform levelCanvas;

    [Header("Shop References")]
    [SerializeField] private RectTransform shopCanvas;

    private void Start()
    {
        Width = Camera.pixelWidth;
        Height = Camera.pixelHeight;

        ScaleTopMenu();
        ScaleMenuPanel();
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
        float scaleFactor = Width / (float)ORIGINAL_WIDTH;
        GameObject resourcePanel = topMenu.transform.GetChild(1).gameObject;
        RectTransform accountFrameRect = topMenu.transform.GetChild(0).GetComponent<RectTransform>(), resourcePanelRect = resourcePanel.GetComponent<RectTransform>();
        RectTransform energyFrameRect = resourcePanelRect.transform.GetChild(0).GetComponent<RectTransform>(),
            moneyFrameRect = resourcePanelRect.transform.GetChild(1).GetComponent<RectTransform>();

        topMenu.sizeDelta = new Vector2(Width, TOP_MENU_HEIGHT);
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

    private void ScaleMenuPanel()
    {
        menuCanvas.offsetMin = new Vector2(-Width, 0);
        menuCanvas.offsetMax = new Vector2(Width, 0);
        viewport.offsetMin = new Vector2(Width, 0);
        viewport.offsetMax = new Vector2(-Width, 0);
        menuParent.sizeDelta = new Vector2(Width * 3, Height);
        menuParent.anchoredPosition = new Vector2(-Width * 1.5f, Height / 2);
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

        foreach (RectTransform frame in weaponFrames)
        {
            GameObject weaponSlot = frame.transform.GetChild(0).gameObject;
            RectTransform weaponName = weaponSlot.transform.GetChild(0).GetComponent<RectTransform>(),
                weaponIcon = weaponSlot.transform.GetChild(2).GetComponent<RectTransform>(),
                upgradeBtn = weaponSlot.transform.GetChild(3).GetComponent<RectTransform>();
            float slotHeight = frame.rect.height - WEAPONS_SLOT_PADDING * 2;

            frame.sizeDelta = new Vector2((Width - ARMORY_SLOT_GAP * 2) / 3, 0);

            if (frame.transform.GetSiblingIndex() == 0)
            {
                frame.anchoredPosition = new Vector2(frame.rect.width / 2, frame.anchoredPosition.y);
            }
            else if (frame.transform.GetSiblingIndex() == 2)
            {
                frame.anchoredPosition = new Vector2(-frame.rect.width / 2, frame.anchoredPosition.y);
            }

            weaponName.sizeDelta = new Vector2(weaponName.sizeDelta.x, slotHeight * WEAPON_NAME_RATIO);
            weaponName.anchoredPosition = new Vector2(0, -weaponName.sizeDelta.y / 2);
            upgradeBtn.sizeDelta = new Vector2(upgradeBtn.sizeDelta.x, slotHeight * UPGRADE_BTN_RATIO);
            upgradeBtn.anchoredPosition = new Vector2(0, upgradeBtn.sizeDelta.y / 2);
            weaponIcon.sizeDelta = new Vector2(weaponIcon.sizeDelta.x, slotHeight - weaponName.sizeDelta.y - upgradeBtn.sizeDelta.y - ARMORY_SLOT_GAP * 2);
            weaponIcon.anchoredPosition = new Vector2(0, upgradeBtn.sizeDelta.y + ARMORY_SLOT_GAP + weaponIcon.sizeDelta.y / 2);
        }

        FrameWidth = weaponFrames[0].rect.width;
        FrameHeight = weaponFrames[0].rect.height;
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
        float scaleFactor = Width / (float)ORIGINAL_WIDTH;
        RectTransform armoryIconRect = navigationMenu.transform.GetChild(0).GetComponent<RectTransform>(),
            mainMenuIconRect = navigationMenu.transform.GetChild(1).GetComponent<RectTransform>(),
            shopIconRect = navigationMenu.transform.GetChild(2).GetComponent<RectTransform>();

        navigationMenu.sizeDelta = new Vector2(Width, ICON_MENU_HEIGHT);
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
