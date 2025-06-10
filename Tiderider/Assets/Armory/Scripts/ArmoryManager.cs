using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Singleton class for managing the armory UI in the game.
/// </summary>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
public class ArmoryManager : MonoBehaviour
{
    public static ArmoryManager Instance { get; private set; }
    private GameObject currentSlot = null; // To keep track of the currently selected slot index

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
    public void SelectSlot(GameObject Slot)
    {
        // Logic to select a slot in the armory
    ;

        if (currentSlot != null && currentSlot == Slot)
        {
            
            DeselectSlot(); // If the same slot is clicked, deselect it
            return;
        }
        currentSlot = Slot;
        Slot.GetComponentInChildren<Image>().color = Slot.GetComponentInChildren<Button>().colors.selectedColor; // Change the color of the slot to indicate selection
        EventSystem.current.SetSelectedGameObject(Slot); // Set the selected GameObject in the EventSystem
    }

    /// <summary>
    /// Deselects the currently selected slot in the armory UI.
    /// </summary>
    public void DeselectSlot()
    {
        //Check if there is a currently selected slot
        if (currentSlot != null)
        {
            currentSlot.GetComponentInChildren<Image>().color = Color.black; //Deselect the slot by changing its color back to black
            EventSystem.current.SetSelectedGameObject(null); // Clear the selected GameObject and the current slot
            currentSlot = null;
        }
    }

    /// <summary>
    /// Selects a weapon for the currently selected slot in the armory UI.
    /// </summary>
    /// <param name="Weapon">Weapon to be selected to the slot</param>
    /// <todo>
    /// Implement logic to show the weapon's info card when no slot is selected.
    /// </todo>
    public void SelectWeapon(GameObject Weapon)
    {
        if (currentSlot == null)
        {
            // Show Info Card of the Weapon
        }
        else
        {
            // Logic to select the weapon in the currently selected slot
            currentSlot.transform.GetComponent<Image>().sprite = Weapon.GetComponent<Image>().sprite; // Set the slot's image to the weapon's image

            //currentSlot.GetComponentInChildren<Image>().color = Color.red; 
        }
    }
}
