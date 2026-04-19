using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ScaleManager : MonoBehaviour
{
    public Camera Camera;
    private const int ORIGINAL_WIDTH = 1080, ORIGINAL_HEIGHT = 2400, TOP_MENU_HEIGHT = 200, TOP_MENU_PADDING = 25, TOP_MENU_GAP = 25, GEAR_ICON_SIZE = 100, ICON_MENU_HEIGHT = 200;
    public static float Width, Height, FrameWidth, FrameHeight, SelectedIconWidth, SideIconWidth;
    public static Color SelectedColor, SideColor;

    [SerializeField] private RectTransform topMenu, navigationMenu;
    [SerializeField] private RectTransform menuCanvas, viewport, menuParent;

    [SerializeField] private RectTransform armoryCanvas, armoryTopCanvas, armoryBottomCanvas, Ship, weaponsPanel, firstRow, secondRow;
    [SerializeField] private RectTransform[] shipSlots, weaponFrames;
    private const int SHIP_SLOT_COUNT = 3, SHIP_SLOT_OFFSET = 25, SHIP_SLOT_GAP = 100, ARMORY_ROW_GAP = 40, ARMORY_SLOT_GAP = 15, WEAPONS_PANEL_MARGIN = 40, WEAPONS_SLOT_PADDING = 25;
    private const float SHIP_RATIO = 0.73f, WEAPON_NAME_RATIO = 0.15f, UPGRADE_BTN_RATIO = 0.3f;

    [SerializeField] private RectTransform levelCanvas, levelMiddleCanvas, levelContent, mapShip;

    [SerializeField] private RectTransform shopCanvas, shopMiddleCanvas, energyRow, weaponRow, coinRow;
    private const float SHOP_SIDE_MARGIN = 65f, SHOP_ROW_HEIGHT_RATIO = 650f / 2000f, SHOP_FRAME_ASPECT = 400f / 650f, SHOP_CONTENT_ASPECT = 1700f / 650f;

    private float scaleX;

    private void Awake()
    {
        Width = Camera.pixelWidth;
        Height = Camera.pixelHeight;
        scaleX = Width / (float)ORIGINAL_WIDTH;

        ScaleTopMenu();
        ScaleMenuPanel();
        ScaleArmory();
        ScaleLevels();
        ScaleShop();
        ScaleNavigationMenu();
        InitializeMapShipPosition();
    }

    private void ScaleTopMenu()
    {
        RectTransform accountFrameRect = topMenu.transform.GetChild(0).GetComponent<RectTransform>();
        RectTransform resourcePanelRect = topMenu.transform.GetChild(1).GetComponent<RectTransform>();
        RectTransform energyFrameRect = resourcePanelRect.transform.GetChild(0).GetComponent<RectTransform>();
        RectTransform moneyFrameRect = resourcePanelRect.transform.GetChild(1).GetComponent<RectTransform>();

        topMenu.sizeDelta = new Vector2(Width, TOP_MENU_HEIGHT);
        topMenu.anchoredPosition = new Vector2(0, -TOP_MENU_HEIGHT / 2);

        accountFrameRect.sizeDelta = new Vector2(accountFrameRect.sizeDelta.x * scaleX, TOP_MENU_HEIGHT);
        accountFrameRect.anchoredPosition = new Vector2(accountFrameRect.sizeDelta.x / 2, 0);

        resourcePanelRect.sizeDelta = new Vector2(resourcePanelRect.sizeDelta.x * scaleX, resourcePanelRect.sizeDelta.y);
        resourcePanelRect.anchoredPosition = new Vector2(-resourcePanelRect.sizeDelta.x / 2 - TOP_MENU_PADDING, 0);

        float resourceWidth = (resourcePanelRect.sizeDelta.x - GEAR_ICON_SIZE - TOP_MENU_GAP * 2) / 2;
        energyFrameRect.sizeDelta = new Vector2(resourceWidth, energyFrameRect.sizeDelta.y);
        energyFrameRect.anchoredPosition = new Vector2(energyFrameRect.sizeDelta.x / 2, 0);

        moneyFrameRect.sizeDelta = new Vector2(resourceWidth, moneyFrameRect.sizeDelta.y);
        moneyFrameRect.anchoredPosition = new Vector2(moneyFrameRect.sizeDelta.x * 1.5f + TOP_MENU_GAP, 0);
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

    private void ScaleArmory()
    {
        armoryCanvas.sizeDelta = new Vector2(Width, Height);
        armoryCanvas.anchoredPosition = new Vector2(-Width, 0);
        armoryTopCanvas.sizeDelta = new Vector2(0, Width);
        armoryTopCanvas.anchoredPosition = new Vector2(0, -Width / 2);
        armoryBottomCanvas.sizeDelta = new Vector2(0, Width);
        armoryBottomCanvas.anchoredPosition = new Vector2(0, Width / 2);

        Ship.sizeDelta = new Vector2(Ship.rect.height * SHIP_RATIO, 0);

        float totalOffset = SHIP_SLOT_OFFSET * (SHIP_SLOT_COUNT - 1) + SHIP_SLOT_GAP * (SHIP_SLOT_COUNT - 1);
        float slotDimension = (Ship.rect.height - totalOffset) / SHIP_SLOT_COUNT;

        for (int i = 0; i < shipSlots.Length; i++)
        {
            shipSlots[i].sizeDelta = new Vector2(slotDimension, slotDimension);
            if (i < 2)
                shipSlots[i].anchoredPosition = new Vector2(shipSlots[i].anchoredPosition.x, -shipSlots[i].rect.height / 2 - SHIP_SLOT_OFFSET);
            else if (i > 3)
                shipSlots[i].anchoredPosition = new Vector2(shipSlots[i].anchoredPosition.x, shipSlots[i].rect.height / 2 + SHIP_SLOT_OFFSET);
        }

        float rowHeight = (armoryBottomCanvas.rect.height - ICON_MENU_HEIGHT - WEAPONS_PANEL_MARGIN - ARMORY_ROW_GAP) / 2;
        firstRow.sizeDelta = new Vector2(0, rowHeight);
        firstRow.anchoredPosition = new Vector2(0, -firstRow.sizeDelta.y / 2);
        secondRow.sizeDelta = new Vector2(0, rowHeight);
        secondRow.anchoredPosition = new Vector2(0, secondRow.sizeDelta.y / 2 + WEAPONS_PANEL_MARGIN);

        float frameWidthVal = (Width - ARMORY_SLOT_GAP * 2) / 3;

        for (int i = 0; i < weaponFrames.Length; i++)
        {
            RectTransform frame = weaponFrames[i];
            GameObject weaponSlot = frame.transform.GetChild(0).gameObject;
            RectTransform weaponName = weaponSlot.transform.GetChild(0).GetComponent<RectTransform>();
            RectTransform weaponIcon = weaponSlot.transform.GetChild(2).GetComponent<RectTransform>();
            RectTransform upgradeBtn = weaponSlot.transform.GetChild(3).GetComponent<RectTransform>();

            float slotHeight = frame.rect.height - WEAPONS_SLOT_PADDING * 2;
            frame.sizeDelta = new Vector2(frameWidthVal, 0);

            if (i == 0) frame.anchoredPosition = new Vector2(frame.rect.width / 2, frame.anchoredPosition.y);
            else if (i == 2) frame.anchoredPosition = new Vector2(-frame.rect.width / 2, frame.anchoredPosition.y);

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

    private void ScaleLevels()
    {
        levelCanvas.sizeDelta = new Vector2(Width, Height);
        levelCanvas.anchoredPosition = Vector2.zero;

        if (levelMiddleCanvas != null)
        {
            levelMiddleCanvas.offsetMax = new Vector2(0, -TOP_MENU_HEIGHT);
            levelMiddleCanvas.offsetMin = new Vector2(0, ICON_MENU_HEIGHT);
        }

        if (levelContent != null)
        {
            levelContent.sizeDelta = new Vector2(levelContent.sizeDelta.x, levelContent.sizeDelta.y * scaleX);
            levelContent.anchoredPosition *= scaleX;

            for (int i = 0; i < levelContent.childCount; i++)
            {
                if (levelContent.GetChild(i) is RectTransform levelNode)
                {
                    levelNode.anchoredPosition *= scaleX;
                    levelNode.sizeDelta *= scaleX;
                }
            }
        }
    }

    private void InitializeMapShipPosition()
    {
        if (levelContent == null || mapShip == null) return;

        int highestUnlocked = Mathf.Clamp(DataManager.GetHighestUnlockedIndex(), 0, levelContent.childCount - 1);

        if (levelContent.GetChild(highestUnlocked) is RectTransform targetLevel)
        {
            mapShip.anchoredPosition = targetLevel.anchoredPosition + new Vector2(-80f * scaleX, -80f * scaleX);
        }
    }

    private void ScaleShop()
    {
        shopCanvas.sizeDelta = new Vector2(Width, Height);
        shopCanvas.anchoredPosition = new Vector2(Width, 0);

        if (shopMiddleCanvas != null)
        {
            shopMiddleCanvas.offsetMax = new Vector2(0, -TOP_MENU_HEIGHT);
            shopMiddleCanvas.offsetMin = new Vector2(0, ICON_MENU_HEIGHT);

            float rowHeight = shopMiddleCanvas.rect.height * SHOP_ROW_HEIGHT_RATIO;
            float sideMargin = SHOP_SIDE_MARGIN * scaleX;

            RectTransform[] rows = { energyRow, weaponRow, coinRow };

            for (int r = 0; r < rows.Length; r++)
            {
                if (rows[r] == null) continue;

                RectTransform row = rows[r];
                row.offsetMin = new Vector2(sideMargin, row.offsetMin.y);
                row.offsetMax = new Vector2(-sideMargin, row.offsetMax.y);
                row.sizeDelta = new Vector2(row.sizeDelta.x, rowHeight);

                if (r == 0) row.anchoredPosition = new Vector2(row.anchoredPosition.x, -rowHeight / 2f);
                else if (r == 1) row.anchoredPosition = new Vector2(row.anchoredPosition.x, 0);
                else if (r == 2) row.anchoredPosition = new Vector2(row.anchoredPosition.x, rowHeight / 2f);

                Transform viewport = row.Find("Viewport");
                if (viewport != null && viewport.Find("Content")?.GetComponent<RectTransform>() is RectTransform content)
                {
                    float contentWidth = rowHeight * SHOP_CONTENT_ASPECT;
                    content.sizeDelta = new Vector2(contentWidth, rowHeight);
                    content.anchoredPosition = new Vector2(contentWidth / 2f, 0);

                    float frameWidthVal = rowHeight * SHOP_FRAME_ASPECT;
                    int frameCount = content.childCount;
                    float gap = frameCount > 1 ? (contentWidth - (frameCount * frameWidthVal)) / (frameCount - 1) : 0;

                    for (int i = 0; i < frameCount; i++)
                    {
                        RectTransform frame = content.GetChild(i).GetComponent<RectTransform>();
                        frame.sizeDelta = new Vector2(frameWidthVal, rowHeight);
                        frame.anchoredPosition = new Vector2((frameWidthVal / 2f) + i * (frameWidthVal + gap), 0);

                        if (frame.childCount >= 4)
                        {
                            float wScale = frameWidthVal / 400f;
                            float hScale = rowHeight / 650f;

                            if (frame.GetChild(0) is RectTransform title)
                            {
                                title.sizeDelta = new Vector2(200f * wScale, 50f * hScale);
                                title.anchoredPosition = new Vector2(0, 275f * hScale);
                            }
                            if (frame.GetChild(1) is RectTransform icon)
                            {
                                icon.sizeDelta = new Vector2(375f * hScale, 375f * hScale);
                                icon.anchoredPosition = new Vector2(0, 37.5f * hScale);
                            }
                            if (frame.GetChild(2) is RectTransform desc)
                            {
                                desc.sizeDelta = new Vector2(200f * wScale, 50f * hScale);
                                desc.anchoredPosition = new Vector2(0, -200f * hScale);
                            }
                            if (frame.GetChild(3) is RectTransform price)
                            {
                                price.sizeDelta = new Vector2(200f * wScale, 50f * hScale);
                                price.anchoredPosition = new Vector2(0, -275f * hScale);
                            }
                        }
                    }
                }
            }
        }
    }

    private void ScaleNavigationMenu()
    {
        RectTransform armoryIconRect = navigationMenu.transform.GetChild(0).GetComponent<RectTransform>();
        RectTransform mainMenuIconRect = navigationMenu.transform.GetChild(1).GetComponent<RectTransform>();
        RectTransform shopIconRect = navigationMenu.transform.GetChild(2).GetComponent<RectTransform>();

        navigationMenu.sizeDelta = new Vector2(Width, ICON_MENU_HEIGHT);
        navigationMenu.anchoredPosition = new Vector2(0, ICON_MENU_HEIGHT / 2);

        armoryIconRect.sizeDelta = new Vector2(armoryIconRect.sizeDelta.x * scaleX, ICON_MENU_HEIGHT);
        armoryIconRect.anchoredPosition = new Vector2(armoryIconRect.sizeDelta.x / 2, 0);
        SideIconWidth = armoryIconRect.sizeDelta.x;
        SideColor = armoryIconRect.GetComponent<Image>().color;

        mainMenuIconRect.sizeDelta = new Vector2(mainMenuIconRect.sizeDelta.x * scaleX, ICON_MENU_HEIGHT);
        mainMenuIconRect.anchoredPosition = Vector2.zero;
        SelectedIconWidth = mainMenuIconRect.sizeDelta.x;
        SelectedColor = mainMenuIconRect.GetComponent<Image>().color;

        shopIconRect.sizeDelta = new Vector2(shopIconRect.sizeDelta.x * scaleX, ICON_MENU_HEIGHT);
        shopIconRect.anchoredPosition = new Vector2(-shopIconRect.sizeDelta.x / 2, 0);
    }
}