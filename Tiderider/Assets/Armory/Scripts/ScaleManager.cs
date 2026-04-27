using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ScaleManager : MonoBehaviour
{
    public Camera Camera;
    private const int ORIGINAL_WIDTH = 1080, ORIGINAL_HEIGHT = 2400, ICON_MENU_HEIGHT = 200;
    private const float NAV_MENU_HEIGHT = 150f;
    private const float TOP_RESOURCE_SIDE_MARGIN = 30f, TOP_RESOURCE_TOP_MARGIN = 30f, TOP_RESOURCE_WIDTH_SCALE = 1.4f;
    private static readonly Color ResourceChipColor = new(0.08f, 0.11f, 0.17f, 0.78f);
    private static readonly Color NavSelectedPanelColor = new(0.69f, 0.52f, 0.20f, 0.78f);
    private static readonly Color NavSidePanelColor = new(0.08f, 0.11f, 0.17f, 0.68f);
    private static readonly Color NavSelectedGlyphColor = new(1f, 0.96f, 0.88f, 1f);
    private static readonly Color NavSideGlyphColor = new(1f, 1f, 1f, 0.68f);
    public static float Width, Height, FrameWidth, FrameHeight, SelectedIconWidth, SideIconWidth;
    public static float VirtualWidth, VirtualHeight, HInset, VInset;
    public static Color SelectedColor, SideColor, SelectedGlyphColor, SideGlyphColor;

    [SerializeField] private RectTransform topMenu, navigationMenu;
    [SerializeField] private RectTransform menuCanvas, viewport, menuParent;
    [SerializeField] private RawImage edgeLeft, edgeRight, edgeTop, edgeBottom;

    [SerializeField] private RectTransform armoryCanvas, armoryTopCanvas, armoryBottomCanvas, Ship, weaponsPanel, firstRow, secondRow;
    [SerializeField] private RectTransform[] shipSlots, weaponFrames;
    private const int SHIP_SLOT_COUNT = 3, SHIP_SLOT_OFFSET = 25, SHIP_SLOT_GAP = 100, ARMORY_ROW_GAP = 40, ARMORY_SLOT_GAP = 15, WEAPONS_PANEL_MARGIN = 40, WEAPONS_SLOT_PADDING = 25;
    private const float SHIP_RATIO = 0.73f, WEAPON_NAME_RATIO = 0.15f, UPGRADE_BTN_RATIO = 0.3f;

    [SerializeField] private RectTransform levelCanvas, levelMiddleCanvas, levelContent, mapShip;

    [SerializeField] private RectTransform shopCanvas, shopMiddleCanvas, energyRow, weaponRow, coinRow;
    private const float SHOP_SIDE_MARGIN = 65f, SHOP_ROW_HEIGHT_RATIO = 650f / 2000f, SHOP_FRAME_ASPECT = 400f / 650f, SHOP_CONTENT_ASPECT = 1700f / 650f;

    private float scaleX;
    private float topInset;
    private float virtualWidth, virtualHeight, hInset, vInset;

    private void Awake()
    {
        Width = Camera.pixelWidth;
        Height = Camera.pixelHeight;

        float originalAspect = (float)ORIGINAL_WIDTH / ORIGINAL_HEIGHT;
        float currentAspect = Width / Height;

        if (currentAspect > originalAspect)
        {
            virtualHeight = Height;
            virtualWidth = Height * originalAspect;
            hInset = (Width - virtualWidth) / 2f;
            vInset = 0f;
        }
        else if (currentAspect < originalAspect)
        {
            virtualWidth = Width;
            virtualHeight = Width / originalAspect;
            hInset = 0f;
            vInset = (Height - virtualHeight) / 2f;
        }
        else
        {
            virtualWidth = Width;
            virtualHeight = Height;
            hInset = 0f;
            vInset = 0f;
        }

        scaleX = virtualWidth / (float)ORIGINAL_WIDTH;

        VirtualWidth = virtualWidth;
        VirtualHeight = virtualHeight;
        HInset = hInset;
        VInset = vInset;

        ScaleTopMenu();
        ScaleMenuPanel();
        ScaleArmory();
        ScaleLevels();
        ScaleShop();
        ScaleNavigationMenu();
        InitializeMapShipPosition();
        ScaleEdgeBars();
    }

    private void ScaleTopMenu()
    {
        Image topMenuImage = topMenu.GetComponent<Image>();
        RectTransform accountFrameRect = topMenu.transform.GetChild(0).GetComponent<RectTransform>();
        RectTransform resourcePanelRect = topMenu.transform.GetChild(1).GetComponent<RectTransform>();
        RectTransform energyFrameRect = resourcePanelRect.transform.GetChild(0).GetComponent<RectTransform>();
        RectTransform moneyFrameRect = resourcePanelRect.transform.GetChild(1).GetComponent<RectTransform>();
        Image energyFrameImage = energyFrameRect.GetComponent<Image>();
        Image moneyFrameImage = moneyFrameRect.GetComponent<Image>();
        RectTransform settingsIconRect = resourcePanelRect.childCount > 2
            ? resourcePanelRect.transform.GetChild(2).GetComponent<RectTransform>()
            : null;

        float frameWidth = energyFrameRect.sizeDelta.x * scaleX * TOP_RESOURCE_WIDTH_SCALE;
        float frameHeight = energyFrameRect.sizeDelta.y * scaleX;
        float sideMargin = TOP_RESOURCE_SIDE_MARGIN * scaleX;
        float topMargin = TOP_RESOURCE_TOP_MARGIN * scaleX;

        topInset = frameHeight + (topMargin * 2f);

        if (topMenuImage != null)
        {
            topMenuImage.enabled = false;
            topMenuImage.raycastTarget = false;
        }

        accountFrameRect.gameObject.SetActive(false);
        if (settingsIconRect != null)
            settingsIconRect.gameObject.SetActive(false);

        topMenu.sizeDelta = new Vector2(virtualWidth, topInset);
        topMenu.anchoredPosition = new Vector2(0, -(topInset / 2f + vInset));

        resourcePanelRect.anchorMin = new Vector2(0f, 0.5f);
        resourcePanelRect.anchorMax = new Vector2(1f, 0.5f);
        resourcePanelRect.pivot = new Vector2(0.5f, 0.5f);
        resourcePanelRect.sizeDelta = new Vector2(0f, frameHeight);
        resourcePanelRect.anchoredPosition = Vector2.zero;

        energyFrameRect.anchorMin = new Vector2(0f, 0.5f);
        energyFrameRect.anchorMax = new Vector2(0f, 0.5f);
        energyFrameRect.pivot = new Vector2(0.5f, 0.5f);
        energyFrameRect.sizeDelta = new Vector2(frameWidth, frameHeight);
        energyFrameRect.anchoredPosition = new Vector2(sideMargin + frameWidth / 2f, 0f);
        if (energyFrameImage != null)
            energyFrameImage.color = ResourceChipColor;

        moneyFrameRect.anchorMin = new Vector2(1f, 0.5f);
        moneyFrameRect.anchorMax = new Vector2(1f, 0.5f);
        moneyFrameRect.pivot = new Vector2(0.5f, 0.5f);
        moneyFrameRect.sizeDelta = new Vector2(frameWidth, frameHeight);
        moneyFrameRect.anchoredPosition = new Vector2(-(sideMargin + frameWidth / 2f), 0f);
        if (moneyFrameImage != null)
            moneyFrameImage.color = ResourceChipColor;
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

        // armoryTopCanvas: top-stretch anchors (0,1)-(1,1)
        // Starts just below the top bar and extends virtualWidth downward, inset by hInset on sides.
        armoryTopCanvas.offsetMax = new Vector2(-hInset, -(topInset + vInset));
        armoryTopCanvas.offsetMin = new Vector2(hInset, -(topInset + vInset + virtualWidth));

        // armoryBottomCanvas: bottom-stretch anchors (0,0)-(1,0)
        // Starts vInset above the screen bottom and extends virtualWidth upward, inset by hInset on sides.
        armoryBottomCanvas.offsetMin = new Vector2(hInset, vInset);
        armoryBottomCanvas.offsetMax = new Vector2(-hInset, vInset + virtualWidth);

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

        float frameWidthVal = (virtualWidth - ARMORY_SLOT_GAP * 2) / 3;

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
            levelMiddleCanvas.offsetMax = new Vector2(-hInset, -topInset - vInset);
            levelMiddleCanvas.offsetMin = new Vector2(hInset, ICON_MENU_HEIGHT + vInset);
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
            shopMiddleCanvas.offsetMax = new Vector2(-hInset, -topInset - vInset);
            shopMiddleCanvas.offsetMin = new Vector2(hInset, ICON_MENU_HEIGHT + vInset);

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

    private void ScaleEdgeBars()
    {
        ApplyEdgeBar(edgeLeft,   new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(hInset, 0f));
        ApplyEdgeBar(edgeRight,  new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(hInset, 0f));
        ApplyEdgeBar(edgeTop,    new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, vInset));
        ApplyEdgeBar(edgeBottom, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, vInset));
    }

    private static void ApplyEdgeBar(RawImage bar, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta)
    {
        if (bar == null) return;
        RectTransform rt = bar.rectTransform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = Vector2.zero;
    }

    private void ScaleNavigationMenu()
    {
        RectTransform armoryIconRect = navigationMenu.transform.GetChild(0).GetComponent<RectTransform>();
        RectTransform mainMenuIconRect = navigationMenu.transform.GetChild(1).GetComponent<RectTransform>();
        RectTransform shopIconRect = navigationMenu.transform.GetChild(2).GetComponent<RectTransform>();
        Image armoryPanelImage = armoryIconRect.GetComponent<Image>();
        Image mainMenuPanelImage = mainMenuIconRect.GetComponent<Image>();
        Image shopPanelImage = shopIconRect.GetComponent<Image>();

        navigationMenu.sizeDelta = new Vector2(virtualWidth, NAV_MENU_HEIGHT * scaleX);
        navigationMenu.anchoredPosition = new Vector2(0, navigationMenu.sizeDelta.y / 2f + vInset);

        armoryIconRect.sizeDelta = new Vector2(armoryIconRect.sizeDelta.x * scaleX, navigationMenu.sizeDelta.y);
        armoryIconRect.anchoredPosition = new Vector2(armoryIconRect.sizeDelta.x / 2, 0);
        SideIconWidth = armoryIconRect.sizeDelta.x;
        SideColor = NavSidePanelColor;

        mainMenuIconRect.sizeDelta = new Vector2(mainMenuIconRect.sizeDelta.x * scaleX, navigationMenu.sizeDelta.y);
        mainMenuIconRect.anchoredPosition = Vector2.zero;
        SelectedIconWidth = mainMenuIconRect.sizeDelta.x;
        SelectedColor = NavSelectedPanelColor;

        shopIconRect.sizeDelta = new Vector2(shopIconRect.sizeDelta.x * scaleX, navigationMenu.sizeDelta.y);
        shopIconRect.anchoredPosition = new Vector2(-shopIconRect.sizeDelta.x / 2, 0);

        SelectedGlyphColor = NavSelectedGlyphColor;
        SideGlyphColor = NavSideGlyphColor;

        if (armoryPanelImage != null)
            armoryPanelImage.color = NavSidePanelColor;
        if (mainMenuPanelImage != null)
            mainMenuPanelImage.color = NavSelectedPanelColor;
        if (shopPanelImage != null)
            shopPanelImage.color = NavSidePanelColor;
    }
}
