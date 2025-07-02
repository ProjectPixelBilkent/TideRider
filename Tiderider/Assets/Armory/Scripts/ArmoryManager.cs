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
                WeaponSlotManager.ShrinkInfoCard(weaponSlot); // Shrink the info card if a weapon slot is selected
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
                WeaponSlotManager.ExpandInfoCard(weaponSlot); // Expand the info card of the weapon
            }
            else if (weaponSlot != WeaponSlot)
            {
                DOTween.Sequence()
                .AppendCallback(() => WeaponSlotManager.ShrinkInfoCard(weaponSlot))
                .AppendInterval(0.5f) // Wait for shrink animation duration
                .AppendCallback(() => {
                    weaponSlot = WeaponSlot;
                    WeaponSlotManager.ExpandInfoCard(weaponSlot);
                });
            }
            else
            {
                WeaponSlotManager.ShrinkInfoCard(weaponSlot);
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
}
