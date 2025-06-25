using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Singleton class for managing the armory UI in the game.
/// </summary>
/// <todo>
/// Implement changing animations for the info card of the weapon when a slot is selected.
/// </todo>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
public class ArmoryManager : MonoBehaviour
{
    public static ArmoryManager Instance { get; private set; }
    private GameObject armorySlot = null, weaponSlot = null; // To keep track of the currently selected slot index
    private Weapon selectedWeapon = null; // To keep track of the currently selected weapon

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    /// <summary>
    /// Selects the slot for weapons in the armory UI.
    /// </summary>
    /// <param name="Slot">Slot buttons parent frame</param>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void SelectSlot(GameObject Slot)
    {
        if (armorySlot != null)
        {
            if (weaponSlot != null)
            {
                ShrinkInfoCard(); // Shrink the info card if a weapon slot is selected
                weaponSlot = null; // Clear the weapon slot variable
            }

            if (armorySlot == Slot)
            {
                DeselectSlot(); // If the same slot is clicked, deselect it
                return;
            }
            else
            {
                DeselectSlot(); // Deselect the previously selected slot
            }
        }

        armorySlot = Slot;
        armorySlot.GetComponent<Image>().color = armorySlot.GetComponent<Button>().colors.pressedColor; // Change the color of the slot to indicate selection
    }

    /// <summary>
    /// Deselects the currently selected slot in the armory UI.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void DeselectSlot()
    {
        //Check if there is a currently selected slot
        if (armorySlot != null)
        {
            armorySlot.GetComponent<Image>().color = Color.white; //Deselect the slot by changing its color back to black
            armorySlot = null;
        }
    }

    /// <summary>
    /// Selects a weapon for the currently selected slot in the armory UI.
    /// </summary>
    /// <param name="Weapon">Weapon to be selected to the slot</param>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void SelectWeapon(GameObject WeaponSlot)
    {
        if (armorySlot == null)
        {
            //Show Info Card of the Weapon
            if (weaponSlot == null)
            {
                weaponSlot = WeaponSlot; // Assign the selected weapon slot to the weapon slot variable
                ExpandInfoCard(); // Expand the info card of the weapon
            }
            else if (weaponSlot != WeaponSlot)
            {
                DOTween.Sequence()
                .AppendCallback(() => ShrinkInfoCard())
                .AppendInterval(0.5f) // Wait for shrink animation duration
                .AppendCallback(() => {
                    weaponSlot = WeaponSlot;
                    ExpandInfoCard();
                });
            }
            else
            {
                ShrinkInfoCard();
                weaponSlot = null; // Clear the weapon slot variable
            }
        }
        else
        {
            // Put the selected weapon into the currently selected slot
            selectedWeapon = WeaponSlot.GetComponent<WeaponSlotManager>().weapon; // Assign the selected weapon to the slot's manager
            armorySlot.GetComponent<Image>().sprite = selectedWeapon.weaponIcon; // Set the icon of the weapon in the slot
            DeselectSlot();
        }
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
    private void ExpandInfoCard()
    {
        // Logic to animate the info card of the weapon
        GameObject parentFrame = weaponSlot.transform.parent.gameObject; // Get the parent of the weapon slot
        int slotIndex = parentFrame.transform.parent.gameObject.transform.GetSiblingIndex() * 3 + parentFrame.transform.GetSiblingIndex(); // Get the index of the slot in the parent
        GameObject slotRow = parentFrame.transform.parent.gameObject; // Get the row of slots
        RectTransform frameRect = parentFrame.GetComponent<RectTransform>(), weaponsPanel = slotRow.transform.parent.gameObject.GetComponent<RectTransform>();
        RectTransform firstSiblingFrame, secondSiblingFrame;
        Vector2 currentPos;
        switch (slotIndex)
        {
            case 0:
                firstSiblingFrame = parentFrame.transform.parent.GetChild(1).gameObject.GetComponent<RectTransform>(); // Get the middle slot as sibling
                secondSiblingFrame = parentFrame.transform.parent.GetChild(2).gameObject.GetComponent<RectTransform>(); // Get the right slot as sibling
                currentPos = frameRect.position;
                frameRect.anchorMin = new Vector2(0.5f, 0f);
                frameRect.anchorMax = new Vector2(0.5f, 1f); // Anchor the selected slot to the center
                frameRect.position = currentPos; // Keep the position of the selected slot
                currentPos = firstSiblingFrame.position;
                firstSiblingFrame.anchorMin = new Vector2(1f, 0f);
                firstSiblingFrame.anchorMax = new Vector2(1f, 1f); // Anchor the middle slot to the right
                firstSiblingFrame.position = currentPos; // Keep the position of the middle slot
                currentPos = secondSiblingFrame.position;
                secondSiblingFrame.anchorMin = new Vector2(1f, 0f);
                secondSiblingFrame.anchorMax = new Vector2(1f, 1f); // Anchor the right slot to the right
                secondSiblingFrame.position = currentPos; // Keep the position of the right slot
                slotRow.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1350f, slotRow.GetComponent<RectTransform>().sizeDelta.y), 0.5f);
                slotRow.GetComponent<RectTransform>().DOAnchorPosX(110f, 0.5f);
                DOTween.To(
                    () => weaponsPanel.offsetMin,
                    x => weaponsPanel.offsetMin = x,
                    new Vector2(weaponsPanel.offsetMin.x, -350f),
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
                    new Vector2(570f, -350f),
                    0.5f
                );
                break;
            case 1:
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
                slotRow.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1350f, slotRow.GetComponent<RectTransform>().sizeDelta.y), 0.5f);
                DOTween.To(
                    () => weaponsPanel.offsetMin,
                    x => weaponsPanel.offsetMin = x,
                    new Vector2(weaponsPanel.offsetMin.x, -350f),
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
                    new Vector2(570f, -350f),
                    0.5f
                );
                break;
            case 2:
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
                frameRect.anchorMin = new Vector2(0.5f, 0f);
                frameRect.anchorMax = new Vector2(0.5f, 1f); // Anchor the selected slot to the center
                frameRect.position = currentPos; // Keep the position of the selected slot
                slotRow.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1350f, slotRow.GetComponent<RectTransform>().sizeDelta.y), 0.5f);
                slotRow.GetComponent<RectTransform>().DOAnchorPosX(-110f, 0.5f);
                DOTween.To(
                    () => weaponsPanel.offsetMin,
                    x => weaponsPanel.offsetMin = x,
                    new Vector2(weaponsPanel.offsetMin.x, -350f),
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
                    new Vector2(570f, -350f),
                    0.5f
                );
                break;
            case 3:
                firstSiblingFrame = parentFrame.transform.parent.GetChild(1).gameObject.GetComponent<RectTransform>(); // Get the middle slot as sibling
                secondSiblingFrame = parentFrame.transform.parent.GetChild(2).gameObject.GetComponent<RectTransform>(); // Get the right slot as sibling
                currentPos = frameRect.position;
                frameRect.anchorMin = new Vector2(0.5f, 0f);
                frameRect.anchorMax = new Vector2(0.5f, 1f); // Anchor the selected slot to the center
                frameRect.position = currentPos; // Keep the position of the selected slot
                currentPos = firstSiblingFrame.position;
                firstSiblingFrame.anchorMin = new Vector2(1f, 0f);
                firstSiblingFrame.anchorMax = new Vector2(1f, 1f); // Anchor the middle slot to the right
                firstSiblingFrame.position = currentPos; // Keep the position of the middle slot
                currentPos = secondSiblingFrame.position;
                secondSiblingFrame.anchorMin = new Vector2(1f, 0f);
                secondSiblingFrame.anchorMax = new Vector2(1f, 1f); // Anchor the right slot to the right
                secondSiblingFrame.position = currentPos; // Keep the position of the right slot
                slotRow.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1350f, slotRow.GetComponent<RectTransform>().sizeDelta.y), 0.5f);
                slotRow.GetComponent<RectTransform>().DOAnchorPosX(110f, 0.5f);
                DOTween.To(
                    () => weaponsPanel.offsetMax,
                    x => weaponsPanel.offsetMax = x,
                    new Vector2(weaponsPanel.offsetMax.x, 350f),
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
                    new Vector2(570f, 350f),
                    0.5f
                );
                break;
            case 4:
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
                slotRow.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1350f, slotRow.GetComponent<RectTransform>().sizeDelta.y), 0.5f);
                DOTween.To(
                    () => weaponsPanel.offsetMax,
                    x => weaponsPanel.offsetMax = x,
                    new Vector2(weaponsPanel.offsetMax.x, 350f),
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
                    new Vector2(570f, 350f),
                    0.5f
                );
                break;
            case 5:
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
                frameRect.anchorMin = new Vector2(0.5f, 0f);
                frameRect.anchorMax = new Vector2(0.5f, 1f); // Anchor the selected slot to the center
                frameRect.position = currentPos; // Keep the position of the selected slot
                slotRow.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1350f, slotRow.GetComponent<RectTransform>().sizeDelta.y), 0.5f);
                slotRow.GetComponent<RectTransform>().DOAnchorPosX(-110f, 0.5f);
                DOTween.To(
                    () => weaponsPanel.offsetMax,
                    x => weaponsPanel.offsetMax = x,
                    new Vector2(weaponsPanel.offsetMax.x, 350f),
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
                    new Vector2(570f, 350f),
                    0.5f
                );
                break;
        }
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
    public void ShrinkInfoCard()
    {
        // Logic to animate the info card of the weapon
        GameObject parentFrame = weaponSlot.transform.parent.gameObject; // Get the parent of the weapon slot
        int slotIndex = parentFrame.transform.parent.gameObject.transform.GetSiblingIndex() * 3 + parentFrame.transform.GetSiblingIndex(); // Get the index of the slot in the parent
        GameObject slotRow = parentFrame.transform.parent.gameObject; // Get the row of slots
        RectTransform frameRect = parentFrame.GetComponent<RectTransform>(), weaponsPanel = slotRow.transform.parent.gameObject.GetComponent<RectTransform>();

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
                    new Vector2(350f, 0f),
                    0.5f
                );
            DOTween.To(
                () => weaponsPanel.offsetMin,
                x => weaponsPanel.offsetMin = x,
                new Vector2(weaponsPanel.offsetMin.x, 0f),
                0.5f
            );
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
                new Vector2(350f, 0f),
                0.5f
            );
            DOTween.To(
                () => weaponsPanel.offsetMax,
                x => weaponsPanel.offsetMax = x,
                new Vector2(weaponsPanel.offsetMax.x, 0f),
                0.5f
            );
        }

        slotRow.GetComponent<RectTransform>().DOAnchorPosX(0f, 0.5f);
        slotRow.GetComponent<RectTransform>().DOSizeDelta(new Vector2(1080f, slotRow.GetComponent<RectTransform>().sizeDelta.y), 0.5f);

        weaponSlot = null; // Clear the weapon slot variable
    }
}
