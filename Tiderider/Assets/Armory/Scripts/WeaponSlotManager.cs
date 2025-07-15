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
    private const float WIDTH_EXPANSION = 270f, HEIGHT_EXPANSION = 350f; // Constants for the expansion of the weapon slot frame

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.GetChild(0).GetComponent<TMP_Text>().text = weapon.weaponName; // Set the name of the weapon in the slot
        transform.GetChild(2).GetComponent<Image>().sprite = weapon.weaponIcon; // Set the icon of the weapon in the slot
    }

    /// <summary>
    /// Method to upgrade the weapon in the slot.
    /// </summary>
    /// <remarks>
    /// Created by: Işık Dönger
    /// </remarks>
    public void UpgradeWeapon()
    {
        DataManager.SubtractCoinAmount(weapon.weaponLevels[(int)typeof(DataManager).GetMethod("Get" + weapon.weaponName + "Level").Invoke(null, null)].cost);
        typeof(DataManager).GetMethod("Increment" + weapon.weaponName + "Level").Invoke(null, null);
    }

    /// <summary>
    /// Expands the info card of the weapon in the armory UI when a slot is selected.
    /// </summary>
    /// <param name="WeaponSlot">Weapon Slot to be expanded</param>
    /// <todo>
    /// Optimize and increase maintainability of the animation logic.
    /// </todo>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void ExpandInfoCard(GameObject weaponSlot)
    {
        // Logic to animate the info card of the weapon
        GameObject parentFrame = weaponSlot.transform.parent.gameObject, weaponDescription = weaponSlot.transform.GetChild(1).gameObject; // Get the parent of the weapon slot
        int slotIndex = parentFrame.transform.parent.gameObject.transform.GetSiblingIndex() * 3 + parentFrame.transform.GetSiblingIndex(); // Get the index of the slot in the parent
        GameObject slotRow = parentFrame.transform.parent.gameObject; // Get the row of slots
        RectTransform frameRect = parentFrame.GetComponent<RectTransform>(), weaponsPanel = slotRow.transform.parent.gameObject.GetComponent<RectTransform>(), rowRect = slotRow.GetComponent<RectTransform>();
        RectTransform firstSiblingFrame, secondSiblingFrame, firstRowRect, secondRowRect;
        Vector2 currentPos;
        float newWidth = ScaleManager.frameWidth + WIDTH_EXPANSION; // New width for the frame

        switch (slotIndex)
        {
            case 0:
                secondRowRect = slotRow.transform.parent.GetChild(1).gameObject.GetComponent<RectTransform>(); // Get the second row of slots
                firstSiblingFrame = parentFrame.transform.parent.GetChild(1).gameObject.GetComponent<RectTransform>(); // Get the middle slot as sibling
                secondSiblingFrame = parentFrame.transform.parent.GetChild(2).gameObject.GetComponent<RectTransform>(); // Get the right slot as sibling
                currentPos = frameRect.position;
                frameRect.anchorMin = new Vector2(0f, 0f);
                frameRect.anchorMax = new Vector2(0f, 1f); // Anchor the selected slot to the left
                frameRect.position = currentPos; // Keep the position of the selected slot
                currentPos = firstSiblingFrame.position;
                firstSiblingFrame.anchorMin = new Vector2(1f, 0f);
                firstSiblingFrame.anchorMax = new Vector2(1f, 1f); // Anchor the middle slot to the right
                firstSiblingFrame.position = currentPos; // Keep the position of the middle slot
                currentPos = secondSiblingFrame.position;
                secondSiblingFrame.anchorMin = new Vector2(1f, 0f);
                secondSiblingFrame.anchorMax = new Vector2(1f, 1f); // Anchor the right slot to the right
                secondSiblingFrame.position = currentPos; // Keep the position of the right slot
                secondRowRect.DOAnchorPosY(secondRowRect.anchoredPosition.y - HEIGHT_EXPANSION, 0.5f); // Move the second row of slots down
                DOTween.To(
                    () => rowRect.offsetMax,
                    x => rowRect.offsetMax = x,
                    new Vector2(WIDTH_EXPANSION, rowRect.offsetMax.y),
                    0.5f
                );
                DOTween.To(
                    () => new Vector2(frameRect.sizeDelta.x, frameRect.offsetMin.y),
                    x =>
                    {
                        frameRect.sizeDelta = new Vector2(x.x, frameRect.sizeDelta.y); // Set width
                        frameRect.offsetMin = new Vector2(frameRect.offsetMin.x, x.y); // Set bottom
                        frameRect.offsetMax = new Vector2(frameRect.offsetMax.x, 0f); // Lock top
                    },
                    new Vector2(newWidth, -HEIGHT_EXPANSION),
                    0.5f
                );
                frameRect.DOAnchorPosX(newWidth / 2, 0.5f); // Move the selected slot to the left side
                break;
            case 1:
                secondRowRect = slotRow.transform.parent.GetChild(1).gameObject.GetComponent<RectTransform>(); // Get the second row of slots
                firstSiblingFrame = parentFrame.transform.parent.GetChild(0).gameObject.GetComponent<RectTransform>(); // Get the middle slot as sibling
                secondSiblingFrame = parentFrame.transform.parent.GetChild(2).gameObject.GetComponent<RectTransform>(); // Get the right slot as sibling
                currentPos = firstSiblingFrame.position;
                firstSiblingFrame.anchorMin = new Vector2(0f, 0f);
                firstSiblingFrame.anchorMax = new Vector2(0f, 1f); // Anchor the left slot to the left
                firstSiblingFrame.position = currentPos; // Keep the position of the left slot
                currentPos = frameRect.position;
                frameRect.anchorMin = new Vector2(0.5f, 0f);
                frameRect.anchorMax = new Vector2(0.5f, 1f); // Anchor the selected slot to the center
                frameRect.position = currentPos; // Keep the position of the selected slot
                currentPos = secondSiblingFrame.position;
                secondSiblingFrame.anchorMin = new Vector2(1f, 0f);
                secondSiblingFrame.anchorMax = new Vector2(1f, 1f); // Anchor the right slot to the right
                secondSiblingFrame.position = currentPos; // Keep the position of the right slot
                secondRowRect.DOAnchorPosY(secondRowRect.anchoredPosition.y - HEIGHT_EXPANSION, 0.5f); // Move the second row of slots down
                DOTween.To(
                    () => rowRect.offsetMin,
                    x => rowRect.offsetMin = x,
                    new Vector2(-WIDTH_EXPANSION / 2, rowRect.offsetMin.y),
                    0.5f
                );
                DOTween.To(
                    () => rowRect.offsetMax,
                    x => rowRect.offsetMax = x,
                    new Vector2(WIDTH_EXPANSION / 2, rowRect.offsetMax.y),
                    0.5f
                );
                DOTween.To(
                    () => new Vector2(frameRect.sizeDelta.x, frameRect.offsetMin.y),
                    x =>
                    {
                        frameRect.sizeDelta = new Vector2(x.x, frameRect.sizeDelta.y); // Set width
                        frameRect.offsetMin = new Vector2(frameRect.offsetMin.x, x.y); // Set bottom
                        frameRect.offsetMax = new Vector2(frameRect.offsetMax.x, 0f); // Lock top
                    },
                    new Vector2(newWidth, -HEIGHT_EXPANSION),
                    0.5f
                );
                break;
            case 2:
                secondRowRect = slotRow.transform.parent.GetChild(1).gameObject.GetComponent<RectTransform>(); // Get the second row of slots
                firstSiblingFrame = parentFrame.transform.parent.GetChild(0).gameObject.GetComponent<RectTransform>(); // Get the middle slot as sibling
                secondSiblingFrame = parentFrame.transform.parent.GetChild(1).gameObject.GetComponent<RectTransform>(); // Get the right slot as sibling
                currentPos = firstSiblingFrame.position;
                firstSiblingFrame.anchorMin = new Vector2(0f, 0f);
                firstSiblingFrame.anchorMax = new Vector2(0f, 1f); // Anchor the left slot to the left
                firstSiblingFrame.position = currentPos; // Keep the position of the left slot
                currentPos = secondSiblingFrame.position;
                secondSiblingFrame.anchorMin = new Vector2(0f, 0f);
                secondSiblingFrame.anchorMax = new Vector2(0f, 1f); // Anchor the middle slot to the left
                secondSiblingFrame.position = currentPos; // Keep the position of the middle slot
                currentPos = frameRect.position;
                frameRect.anchorMin = new Vector2(1f, 0f);
                frameRect.anchorMax = new Vector2(1f, 1f); // Anchor the selected slot to the right
                frameRect.position = currentPos; // Keep the position of the selected slot
                secondRowRect.DOAnchorPosY(secondRowRect.anchoredPosition.y - HEIGHT_EXPANSION, 0.5f); // Move the second row of slots down
                DOTween.To(
                    () => rowRect.offsetMin,
                    x => rowRect.offsetMin = x,
                    new Vector2(-WIDTH_EXPANSION, rowRect.offsetMin.y),
                    0.5f
                );
                DOTween.To(
                    () => new Vector2(frameRect.sizeDelta.x, frameRect.offsetMin.y),
                    x =>
                    {
                        frameRect.sizeDelta = new Vector2(x.x, frameRect.sizeDelta.y); // Set width
                        frameRect.offsetMin = new Vector2(frameRect.offsetMin.x, x.y); // Set bottom
                        frameRect.offsetMax = new Vector2(frameRect.offsetMax.x, 0f); // Lock top
                    },
                    new Vector2(newWidth, -HEIGHT_EXPANSION),
                    0.5f
                );
                frameRect.DOAnchorPosX(-newWidth / 2, 0.5f); // Move the selected slot to the right side
                break;
            case 3:
                firstRowRect = slotRow.transform.parent.GetChild(0).gameObject.GetComponent<RectTransform>(); // Get the first row of slots
                firstSiblingFrame = parentFrame.transform.parent.GetChild(1).gameObject.GetComponent<RectTransform>(); // Get the middle slot as sibling
                secondSiblingFrame = parentFrame.transform.parent.GetChild(2).gameObject.GetComponent<RectTransform>(); // Get the right slot as sibling
                currentPos = frameRect.position;
                frameRect.anchorMin = new Vector2(0f, 0f);
                frameRect.anchorMax = new Vector2(0f, 1f); // Anchor the selected slot to the left
                frameRect.position = currentPos; // Keep the position of the selected slot
                currentPos = firstSiblingFrame.position;
                firstSiblingFrame.anchorMin = new Vector2(1f, 0f);
                firstSiblingFrame.anchorMax = new Vector2(1f, 1f); // Anchor the middle slot to the right
                firstSiblingFrame.position = currentPos; // Keep the position of the middle slot
                currentPos = secondSiblingFrame.position;
                secondSiblingFrame.anchorMin = new Vector2(1f, 0f);
                secondSiblingFrame.anchorMax = new Vector2(1f, 1f); // Anchor the right slot to the right
                secondSiblingFrame.position = currentPos; // Keep the position of the right slot
                firstRowRect.DOAnchorPosY(firstRowRect.anchoredPosition.y + HEIGHT_EXPANSION, 0.5f); // Move the first row of slots up
                DOTween.To(
                    () => rowRect.offsetMax,
                    x => rowRect.offsetMax = x,
                    new Vector2(WIDTH_EXPANSION, rowRect.offsetMax.y),
                    0.5f
                );
                DOTween.To(
                    () => new Vector2(frameRect.sizeDelta.x, frameRect.offsetMax.y),
                    x =>
                    {
                        frameRect.sizeDelta = new Vector2(x.x, frameRect.sizeDelta.y); // Set width
                        frameRect.offsetMax = new Vector2(frameRect.offsetMax.x, x.y); // Set TOP
                        frameRect.offsetMin = new Vector2(frameRect.offsetMin.x, 0f); // Lock BOTTOM
                    },
                    new Vector2(newWidth, HEIGHT_EXPANSION),
                    0.5f
                );
                frameRect.DOAnchorPosX(newWidth / 2, 0.5f); // Move the selected slot to the left side
                break;
            case 4:
                firstRowRect = slotRow.transform.parent.GetChild(0).gameObject.GetComponent<RectTransform>(); // Get the first row of slots
                firstSiblingFrame = parentFrame.transform.parent.GetChild(0).gameObject.GetComponent<RectTransform>(); // Get the middle slot as sibling
                secondSiblingFrame = parentFrame.transform.parent.GetChild(2).gameObject.GetComponent<RectTransform>(); // Get the right slot as sibling
                currentPos = firstSiblingFrame.position;
                firstSiblingFrame.anchorMin = new Vector2(0f, 0f);
                firstSiblingFrame.anchorMax = new Vector2(0f, 1f); // Anchor the left slot to the left
                firstSiblingFrame.position = currentPos; // Keep the position of the left slot
                currentPos = frameRect.position;
                frameRect.anchorMin = new Vector2(0.5f, 0f);
                frameRect.anchorMax = new Vector2(0.5f, 1f); // Anchor the selected slot to the center
                frameRect.position = currentPos; // Keep the position of the selected slot
                currentPos = secondSiblingFrame.position;
                secondSiblingFrame.anchorMin = new Vector2(1f, 0f);
                secondSiblingFrame.anchorMax = new Vector2(1f, 1f); // Anchor the right slot to the right
                secondSiblingFrame.position = currentPos; // Keep the position of the right slot
                firstRowRect.DOAnchorPosY(firstRowRect.anchoredPosition.y + HEIGHT_EXPANSION, 0.5f); // Move the first row of slots up
                DOTween.To(
                    () => rowRect.offsetMin,
                    x => rowRect.offsetMin = x,
                    new Vector2(-WIDTH_EXPANSION / 2, rowRect.offsetMin.y),
                    0.5f
                );
                DOTween.To(
                    () => rowRect.offsetMax,
                    x => rowRect.offsetMax = x,
                    new Vector2(WIDTH_EXPANSION / 2, rowRect.offsetMax.y),
                    0.5f
                );
                DOTween.To(
                    () => new Vector2(frameRect.sizeDelta.x, frameRect.offsetMax.y),
                    x =>
                    {
                        frameRect.sizeDelta = new Vector2(x.x, frameRect.sizeDelta.y); // Set width
                        frameRect.offsetMax = new Vector2(frameRect.offsetMax.x, x.y); // Set TOP
                        frameRect.offsetMin = new Vector2(frameRect.offsetMin.x, 0f); // Lock BOTTOM
                    },
                    new Vector2(newWidth, HEIGHT_EXPANSION),
                    0.5f
                );
                break;
            case 5:
                firstRowRect = slotRow.transform.parent.GetChild(0).gameObject.GetComponent<RectTransform>(); // Get the first row of slots
                firstSiblingFrame = parentFrame.transform.parent.GetChild(0).gameObject.GetComponent<RectTransform>(); // Get the middle slot as sibling
                secondSiblingFrame = parentFrame.transform.parent.GetChild(1).gameObject.GetComponent<RectTransform>(); // Get the right slot as sibling
                currentPos = firstSiblingFrame.position;
                firstSiblingFrame.anchorMin = new Vector2(0f, 0f);
                firstSiblingFrame.anchorMax = new Vector2(0f, 1f); // Anchor the left slot to the left
                firstSiblingFrame.position = currentPos; // Keep the position of the left slot
                currentPos = secondSiblingFrame.position;
                secondSiblingFrame.anchorMin = new Vector2(0f, 0f);
                secondSiblingFrame.anchorMax = new Vector2(0f, 1f); // Anchor the middle slot to the left
                secondSiblingFrame.position = currentPos; // Keep the position of the middle slot
                currentPos = frameRect.position;
                frameRect.anchorMin = new Vector2(1f, 0f);
                frameRect.anchorMax = new Vector2(1f, 1f); // Anchor the selected slot to the right
                frameRect.position = currentPos; // Keep the position of the selected slot
                firstRowRect.DOAnchorPosY(firstRowRect.anchoredPosition.y + HEIGHT_EXPANSION, 0.5f); // Move the first row of slots up
                DOTween.To(
                    () => rowRect.offsetMin,
                    x => rowRect.offsetMin = x,
                    new Vector2(-WIDTH_EXPANSION, rowRect.offsetMin.y),
                    0.5f
                );
                DOTween.To(
                    () => new Vector2(frameRect.sizeDelta.x, frameRect.offsetMax.y),
                    x =>
                    {
                        frameRect.sizeDelta = new Vector2(x.x, frameRect.sizeDelta.y); // Set width
                        frameRect.offsetMax = new Vector2(frameRect.offsetMax.x, x.y); // Set TOP
                        frameRect.offsetMin = new Vector2(frameRect.offsetMin.x, 0f); // Lock BOTTOM
                    },
                    new Vector2(newWidth, HEIGHT_EXPANSION),
                    0.5f
                );
                frameRect.DOAnchorPosX(-newWidth / 2, 0.5f); // Move the selected slot to the left side
                break;
        }

        weaponDescription.GetComponent<TMP_Text>().text = weaponSlot.GetComponent<WeaponSlotManager>().weapon.weaponDescription; // Set the description of the weapon in the slot
    }

    /// <summary>
    /// Shrinks the info card of the weapon in the armory UI when a slot is selected.
    /// </summary>
    /// <param name="WeaponSlot">Weapon Slot to be expanded</param>
    /// <todo>
    /// Optimize and increase maintainability of the animation logic.
    /// </todo>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void ShrinkInfoCard(GameObject weaponSlot)
    {
        // Logic to animate the info card of the weapon
        GameObject parentFrame = weaponSlot.transform.parent.gameObject, weaponDescription = weaponSlot.transform.GetChild(1).gameObject; // Get the parent of the weapon slot
        int slotIndex = parentFrame.transform.parent.gameObject.transform.GetSiblingIndex() * 3 + parentFrame.transform.GetSiblingIndex(); // Get the index of the slot in the parent
        GameObject slotRow = parentFrame.transform.parent.gameObject; // Get the row of slots
        RectTransform frameRect = parentFrame.GetComponent<RectTransform>(), weaponsPanel = slotRow.transform.parent.gameObject.GetComponent<RectTransform>(),
            firstRowRect = slotRow.transform.parent.GetChild(0).gameObject.GetComponent<RectTransform>(),
            secondRowRect = slotRow.transform.parent.GetChild(1).gameObject.GetComponent<RectTransform>();

        if (slotIndex < 3)
        {
            DOTween.To(
                () => new Vector2(frameRect.sizeDelta.x, frameRect.offsetMin.y),
                x =>
                {
                    frameRect.sizeDelta = new Vector2(x.x, frameRect.sizeDelta.y); // Set width
                    frameRect.offsetMin = new Vector2(frameRect.offsetMin.x, x.y); // Set bottom
                    frameRect.offsetMax = new Vector2(frameRect.offsetMax.x, 0f); // Lock top
                },
                new Vector2(ScaleManager.frameWidth, 0f),
                0.5f
            );
            DOTween.To(
                () => firstRowRect.offsetMin,
                x => firstRowRect.offsetMin = x,
                new Vector2(0f, firstRowRect.offsetMin.y),
                0.5f
            );
            DOTween.To(
                () => firstRowRect.offsetMax,
                x => firstRowRect.offsetMax = x,
                new Vector2(0f, firstRowRect.offsetMax.y),
                0.5f
            );
            secondRowRect.DOAnchorPosY(secondRowRect.anchoredPosition.y + HEIGHT_EXPANSION, 0.5f); // Move the second row of slots back up
        }
        else
        {
            DOTween.To(
                () => new Vector2(frameRect.sizeDelta.x, frameRect.offsetMax.y),
                x =>
                {
                    frameRect.sizeDelta = new Vector2(x.x, frameRect.sizeDelta.y); // Set width
                    frameRect.offsetMax = new Vector2(frameRect.offsetMax.x, x.y); // Set TOP
                    frameRect.offsetMin = new Vector2(frameRect.offsetMin.x, 0f); // Lock BOTTOM
                },
                new Vector2(ScaleManager.frameWidth, 0f),
                0.5f
            );
            DOTween.To(
                () => secondRowRect.offsetMin,
                x => secondRowRect.offsetMin = x,
                new Vector2(0f, secondRowRect.offsetMin.y),
                0.5f
            );
            DOTween.To(
                () => secondRowRect.offsetMax,
                x => secondRowRect.offsetMax = x,
                new Vector2(0f, secondRowRect.offsetMax.y),
                0.5f
            );
            firstRowRect.DOAnchorPosY(firstRowRect.anchoredPosition.y - HEIGHT_EXPANSION, 0.5f); // Move the first row of slots back down
        }

        if (slotIndex == 0 || slotIndex == 3)
        {
            frameRect.DOAnchorPosX(ScaleManager.frameWidth / 2, 0.5f); // Move the selected slot back to the center
        }
        else if (slotIndex == 2 || slotIndex == 5)
        {
            frameRect.DOAnchorPosX(-ScaleManager.frameWidth / 2, 0.5f); // Move the selected slot back to the center
        }

        weaponDescription.GetComponent<TMP_Text>().text = ""; // Clear the description of the weapon in the slot
    }
}
