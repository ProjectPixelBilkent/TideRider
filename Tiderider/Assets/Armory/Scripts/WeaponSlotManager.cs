using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using DG.Tweening;
using TMPro;

/// <summary>
/// MonoBehaviour class for managing the weapon slots in the game.
/// </summary>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
public class WeaponSlotManager : MonoBehaviour
{
    public Weapon weapon;
    private GameObject weaponName, weaponDescription, weaponIcon, upgradeBtn;

    // --- STATE MANAGEMENT & CACHE ---
    private static GameObject activeSlot = null;
    private static Sequence uiSequence;
    private static bool layoutCached = false;
    private static bool isShrinking = false;
    private static Action onShrinkCompleteAction = null; // Used to queue seamless card swapping

    // Absolute values to prevent math drift during spam-clicking
    private static float row1BaseY, row2BaseY;
    private static float baseNameHeight, baseIconHeight, baseUpgradeBtnHeight;

    private const float SLOT_PADDING = 25f, SLOT_GAP = 25f, WIDTH_EXPANSION = 270f, HEIGHT_EXPANSION = 350f,
        NAME_EXPANSION = 50f, ICON_EXPANSION = 80f, UPGRADE_EXPANSION = 25f;

    void Start()
    {
        weaponName = transform.GetChild(0).gameObject;
        weaponDescription = transform.GetChild(1).gameObject;
        weaponIcon = transform.GetChild(2).gameObject;
        upgradeBtn = transform.GetChild(3).gameObject;

        weaponName.GetComponent<TMP_Text>().text = weapon.weaponName;
        weaponDescription.GetComponent<TMP_Text>().text = weapon.weaponDescription;
        weaponDescription.SetActive(false);
        weaponIcon.GetComponent<Image>().sprite = weapon.weaponIcon;

        // Reset static locks if the scene reloads
        activeSlot = null;
        layoutCached = false;
        isShrinking = false;
        onShrinkCompleteAction = null;
        if (uiSequence != null) uiSequence.Kill();
    }

    public void UpgradeWeapon()
    {
        DataManager.SubtractCoinAmount(weapon.weaponLevels[(int)typeof(DataManager).GetMethod("Get" + weapon.weaponName + "Level").Invoke(null, null)].cost);
    }

    public static void ExpandInfoCard(GameObject weaponSlot)
    {
        // 1. SEAMLESS SWAP LOGIC: If a different card is open, shrink it and queue this one to expand
        if (activeSlot != null && activeSlot != weaponSlot)
        {
            GameObject nextSlotToOpen = weaponSlot; // Capture for the queue
            onShrinkCompleteAction = () => { ExpandInfoCard(nextSlotToOpen); };
            ShrinkInfoCard(activeSlot);
            return;
        }

        // 2. Prevent double-expanding the same card, UNLESS it's currently shrinking (allows fast reversal)
        if (activeSlot == weaponSlot && !isShrinking) return;

        activeSlot = weaponSlot;
        isShrinking = false;

        GameObject parentFrame = weaponSlot.transform.parent.gameObject;
        Transform slotRowTransform = parentFrame.transform.parent;
        int slotIndex = slotRowTransform.GetSiblingIndex() * 3 + parentFrame.transform.GetSiblingIndex();

        bool isTopRow = slotIndex < 3;
        int columnIndex = slotIndex % 3;

        RectTransform frameRect = parentFrame.GetComponent<RectTransform>();
        RectTransform rowRect = slotRowTransform.GetComponent<RectTransform>();
        RectTransform firstRowRect = slotRowTransform.parent.GetChild(0).GetComponent<RectTransform>();
        RectTransform secondRowRect = slotRowTransform.parent.GetChild(1).GetComponent<RectTransform>();

        // 3. CACHE ABSOLUTE HEIGHTS ON FIRST USE (Stops UI from breaking on rapid clicks)
        if (!layoutCached)
        {
            row1BaseY = firstRowRect.anchoredPosition.y;
            row2BaseY = secondRowRect.anchoredPosition.y;

            baseNameHeight = weaponSlot.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta.y;
            baseIconHeight = weaponSlot.transform.GetChild(2).GetComponent<RectTransform>().sizeDelta.y;
            baseUpgradeBtnHeight = weaponSlot.transform.GetChild(3).GetComponent<RectTransform>().sizeDelta.y;

            layoutCached = true;
        }

        if (uiSequence != null) uiSequence.Kill();
        uiSequence = DOTween.Sequence();

        RectTransform weaponNameRect = weaponSlot.transform.GetChild(0).GetComponent<RectTransform>();
        RectTransform weaponDescriptionRect = weaponSlot.transform.GetChild(1).GetComponent<RectTransform>();
        RectTransform weaponIconRect = weaponSlot.transform.GetChild(2).GetComponent<RectTransform>();
        RectTransform upgradeBtnRect = weaponSlot.transform.GetChild(3).GetComponent<RectTransform>();

        weaponDescriptionRect.gameObject.SetActive(true);

        float newWidth = ScaleManager.FrameWidth + WIDTH_EXPANSION;
        float newHeight = ScaleManager.FrameHeight - SLOT_PADDING * 2;

        // --- HORIZONTAL EXPANSION ---
        RectTransform sibling1, sibling2;
        if (columnIndex == 0) // Left
        {
            SetAnchorKeepPosition(frameRect, 0f, 0f);
            sibling1 = slotRowTransform.GetChild(1).GetComponent<RectTransform>();
            sibling2 = slotRowTransform.GetChild(2).GetComponent<RectTransform>();
            SetAnchorKeepPosition(sibling1, 1f, 1f); SetAnchorKeepPosition(sibling2, 1f, 1f);

            uiSequence.Join(DOTween.To(() => rowRect.offsetMax, x => rowRect.offsetMax = x, new Vector2(WIDTH_EXPANSION, rowRect.offsetMax.y), 0.5f));
            uiSequence.Join(frameRect.DOAnchorPosX(newWidth / 2f, 0.5f));
        }
        else if (columnIndex == 1) // Center
        {
            SetAnchorKeepPosition(frameRect, 0.5f, 0.5f);
            sibling1 = slotRowTransform.GetChild(0).GetComponent<RectTransform>();
            sibling2 = slotRowTransform.GetChild(2).GetComponent<RectTransform>();
            SetAnchorKeepPosition(sibling1, 0f, 0f); SetAnchorKeepPosition(sibling2, 1f, 1f);

            uiSequence.Join(DOTween.To(() => rowRect.offsetMin, x => rowRect.offsetMin = x, new Vector2(-WIDTH_EXPANSION / 2f, rowRect.offsetMin.y), 0.5f));
            uiSequence.Join(DOTween.To(() => rowRect.offsetMax, x => rowRect.offsetMax = x, new Vector2(WIDTH_EXPANSION / 2f, rowRect.offsetMax.y), 0.5f));
        }
        else // Right
        {
            SetAnchorKeepPosition(frameRect, 1f, 1f);
            sibling1 = slotRowTransform.GetChild(0).GetComponent<RectTransform>();
            sibling2 = slotRowTransform.GetChild(1).GetComponent<RectTransform>();
            SetAnchorKeepPosition(sibling1, 0f, 0f); SetAnchorKeepPosition(sibling2, 0f, 0f);

            uiSequence.Join(DOTween.To(() => rowRect.offsetMin, x => rowRect.offsetMin = x, new Vector2(-WIDTH_EXPANSION, rowRect.offsetMin.y), 0.5f));
            uiSequence.Join(frameRect.DOAnchorPosX(-newWidth / 2f, 0.5f));
        }

        // --- VERTICAL EXPANSION ---
        if (isTopRow)
        {
            uiSequence.Join(secondRowRect.DOAnchorPosY(row2BaseY - HEIGHT_EXPANSION, 0.5f));
            uiSequence.Join(DOTween.To(
                () => new Vector2(frameRect.sizeDelta.x, frameRect.offsetMin.y),
                x => { frameRect.sizeDelta = new Vector2(x.x, frameRect.sizeDelta.y); frameRect.offsetMin = new Vector2(frameRect.offsetMin.x, x.y); frameRect.offsetMax = new Vector2(frameRect.offsetMax.x, 0f); },
                new Vector2(newWidth, -HEIGHT_EXPANSION), 0.5f));
        }
        else
        {
            uiSequence.Join(firstRowRect.DOAnchorPosY(row1BaseY + HEIGHT_EXPANSION, 0.5f));
            uiSequence.Join(DOTween.To(
                () => new Vector2(frameRect.sizeDelta.x, frameRect.offsetMax.y),
                x => { frameRect.sizeDelta = new Vector2(x.x, frameRect.sizeDelta.y); frameRect.offsetMax = new Vector2(frameRect.offsetMax.x, x.y); frameRect.offsetMin = new Vector2(frameRect.offsetMin.x, 0f); },
                new Vector2(newWidth, HEIGHT_EXPANSION), 0.5f));
        }

        // --- INTERNAL UI ELEMENTS (Targeting Absolute Numbers) ---
        float targetNameHeight = baseNameHeight + NAME_EXPANSION;
        float targetIconHeight = baseIconHeight + ICON_EXPANSION;
        float targetUpgradeBtnHeight = baseUpgradeBtnHeight + UPGRADE_EXPANSION;
        float descriptionHeight = (newHeight + HEIGHT_EXPANSION) - SLOT_GAP * 3 - targetNameHeight - targetIconHeight - targetUpgradeBtnHeight;

        uiSequence.Join(weaponNameRect.DOSizeDelta(new Vector2(weaponNameRect.sizeDelta.x, targetNameHeight), 0.5f));
        uiSequence.Join(weaponNameRect.DOAnchorPosY(-targetNameHeight / 2f, 0.5f));
        uiSequence.Join(upgradeBtnRect.DOSizeDelta(new Vector2(upgradeBtnRect.sizeDelta.x, targetUpgradeBtnHeight), 0.5f));
        uiSequence.Join(upgradeBtnRect.DOAnchorPosY(targetUpgradeBtnHeight / 2f, 0.5f));
        uiSequence.Join(weaponIconRect.DOSizeDelta(new Vector2(weaponIconRect.sizeDelta.x, targetIconHeight), 0.5f));
        uiSequence.Join(weaponIconRect.DOAnchorPosY(targetUpgradeBtnHeight + SLOT_GAP + targetIconHeight / 2f, 0.5f));
        uiSequence.Join(weaponDescriptionRect.DOSizeDelta(new Vector2(weaponDescriptionRect.sizeDelta.x, descriptionHeight), 0.5f));
        uiSequence.Join(weaponDescriptionRect.DOAnchorPosY(-(targetNameHeight + SLOT_GAP + descriptionHeight / 2f), 0.5f));
    }

    public static void ShrinkInfoCard(GameObject weaponSlot)
    {
        if (activeSlot != weaponSlot) return;

        isShrinking = true;

        RectTransform weaponDescriptionRect = weaponSlot.transform.GetChild(1).GetComponent<RectTransform>();
        weaponDescriptionRect.gameObject.SetActive(false); // Hide instantly

        GameObject parentFrame = weaponSlot.transform.parent.gameObject;
        Transform slotRowTransform = parentFrame.transform.parent;
        int slotIndex = slotRowTransform.GetSiblingIndex() * 3 + parentFrame.transform.GetSiblingIndex();

        bool isTopRow = slotIndex < 3;
        int columnIndex = slotIndex % 3;

        RectTransform frameRect = parentFrame.GetComponent<RectTransform>();
        RectTransform firstRowRect = slotRowTransform.parent.GetChild(0).GetComponent<RectTransform>();
        RectTransform secondRowRect = slotRowTransform.parent.GetChild(1).GetComponent<RectTransform>();

        RectTransform weaponNameRect = weaponSlot.transform.GetChild(0).GetComponent<RectTransform>();
        RectTransform weaponIconRect = weaponSlot.transform.GetChild(2).GetComponent<RectTransform>();
        RectTransform upgradeBtnRect = weaponSlot.transform.GetChild(3).GetComponent<RectTransform>();

        if (uiSequence != null) uiSequence.Kill();
        uiSequence = DOTween.Sequence();

        // --- ROW SHRINKING ---
        if (isTopRow)
        {
            uiSequence.Join(DOTween.To(
                () => new Vector2(frameRect.sizeDelta.x, frameRect.offsetMin.y),
                x => { frameRect.sizeDelta = new Vector2(x.x, frameRect.sizeDelta.y); frameRect.offsetMin = new Vector2(frameRect.offsetMin.x, x.y); frameRect.offsetMax = new Vector2(frameRect.offsetMax.x, 0f); },
                new Vector2(ScaleManager.FrameWidth, 0f), 0.5f));

            uiSequence.Join(DOTween.To(() => firstRowRect.offsetMin, x => firstRowRect.offsetMin = x, new Vector2(0f, firstRowRect.offsetMin.y), 0.5f));
            uiSequence.Join(DOTween.To(() => firstRowRect.offsetMax, x => firstRowRect.offsetMax = x, new Vector2(0f, firstRowRect.offsetMax.y), 0.5f));
            uiSequence.Join(secondRowRect.DOAnchorPosY(row2BaseY, 0.5f));
        }
        else
        {
            uiSequence.Join(DOTween.To(
                () => new Vector2(frameRect.sizeDelta.x, frameRect.offsetMax.y),
                x => { frameRect.sizeDelta = new Vector2(x.x, frameRect.sizeDelta.y); frameRect.offsetMax = new Vector2(frameRect.offsetMax.x, x.y); frameRect.offsetMin = new Vector2(frameRect.offsetMin.x, 0f); },
                new Vector2(ScaleManager.FrameWidth, 0f), 0.5f));

            uiSequence.Join(DOTween.To(() => secondRowRect.offsetMin, x => secondRowRect.offsetMin = x, new Vector2(0f, secondRowRect.offsetMin.y), 0.5f));
            uiSequence.Join(DOTween.To(() => secondRowRect.offsetMax, x => secondRowRect.offsetMax = x, new Vector2(0f, secondRowRect.offsetMax.y), 0.5f));
            uiSequence.Join(firstRowRect.DOAnchorPosY(row1BaseY, 0.5f));
        }

        // --- HORIZONTAL CENTERING ---
        if (columnIndex == 0) uiSequence.Join(frameRect.DOAnchorPosX(ScaleManager.FrameWidth / 2f, 0.5f));
        else if (columnIndex == 2) uiSequence.Join(frameRect.DOAnchorPosX(-ScaleManager.FrameWidth / 2f, 0.5f));

        // --- INTERNAL UI ELEMENTS (Returning to Absolute Base Values) ---
        uiSequence.Join(weaponNameRect.DOSizeDelta(new Vector2(weaponNameRect.sizeDelta.x, baseNameHeight), 0.5f));
        uiSequence.Join(weaponNameRect.DOAnchorPosY(-baseNameHeight / 2f, 0.5f));
        uiSequence.Join(upgradeBtnRect.DOSizeDelta(new Vector2(upgradeBtnRect.sizeDelta.x, baseUpgradeBtnHeight), 0.5f));
        uiSequence.Join(upgradeBtnRect.DOAnchorPosY(baseUpgradeBtnHeight / 2f, 0.5f));
        uiSequence.Join(weaponIconRect.DOSizeDelta(new Vector2(weaponIconRect.sizeDelta.x, baseIconHeight), 0.5f));
        uiSequence.Join(weaponIconRect.DOAnchorPosY(baseUpgradeBtnHeight + SLOT_GAP + baseIconHeight / 2f, 0.5f));

        // --- COMPLETION CALLBACKS ---
        uiSequence.OnComplete(() => {
            isShrinking = false;
            activeSlot = null;

            // If another card was queued while shrinking, trigger it now!
            onShrinkCompleteAction?.Invoke();
            onShrinkCompleteAction = null;
        });
    }

    private static void SetAnchorKeepPosition(RectTransform rt, float anchorMinX, float anchorMaxX)
    {
        Vector2 currentPosition = rt.position;
        rt.anchorMin = new Vector2(anchorMinX, 0f);
        rt.anchorMax = new Vector2(anchorMaxX, 1f);
        rt.position = currentPosition;
    }
}