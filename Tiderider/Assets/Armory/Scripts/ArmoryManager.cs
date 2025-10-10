using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
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
    private GameObject shipSlot = null, weaponSlot = null; // To keep track of the currently selected slot index
    private Weapon selectedWeapon = null; // To keep track of the currently selected weapon
    [SerializeField] private Weapon[] weaponList = new Weapon[6];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayerPrefs.DeleteKey("PlayerArmory");
        if (!PlayerPrefs.HasKey("PlayerArmory"))
        {
            InitPlayerArmory();
        }
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

    private void InitPlayerArmory()
    {
        PlayerPrefs.SetString("PlayerArmory", "Canon|Canon|Canon|Canon|Canon|Canon");
        Debug.Log(PlayerPrefs.GetString("PlayerArmory"));
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
        if (shipSlot != null)
        {
            if (weaponSlot != null)
            {
                WeaponSlotManager.ShrinkInfoCard(weaponSlot); // Shrink the info card if a weapon slot is selected
                weaponSlot = null; // Clear the weapon slot variable
            }

            if (shipSlot == Slot)
            {
                DeselectSlot(); // If the same slot is clicked, deselect it
                return;
            }
            else
            {
                DeselectSlot(); // Deselect the previously selected slot
            }
        }

        shipSlot = Slot;
        shipSlot.GetComponent<Image>().color = shipSlot.GetComponent<Button>().colors.pressedColor; // Change the color of the slot to indicate selection
    }

    /// <summary>
    /// Deselects the currently selected slot in the armory UI.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void DeselectSlot(float delay = 0.1f)
    {
        StartCoroutine(DeselectShipSlotCoroutine(delay));
    }

    /// <summary>
    /// Deselects the currently selected ship slot in the armory UI.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    private IEnumerator DeselectShipSlotCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay); // To prevent conflict with SelectWeapon method

        if (shipSlot != null)
        {
            shipSlot.GetComponent<Image>().color = Color.white; //Deselect the slot by changing its color back to black
            shipSlot = null;
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
        if (shipSlot == null)
        {
            //Show Info Card of the Weapon
            if (weaponSlot == null)
            {
                weaponSlot = WeaponSlot; // Assign the selected weapon slot to the weapon slot variable
                Debug.Log("Weapon Set To: " + weaponSlot.GetComponent<WeaponSlotManager>().weapon);
                WeaponSlotManager.ExpandInfoCard(weaponSlot); // Expand the info card of the weapon
            }
            else if (weaponSlot != WeaponSlot)
            {
                Debug.Log("Previous Weapon Was: " + weaponSlot.GetComponent <WeaponSlotManager>().weapon);
                DOTween.Sequence()
                .AppendCallback(() => WeaponSlotManager.ShrinkInfoCard(weaponSlot))
                .AppendInterval(0.5f) // Wait for shrink animation duration
                .AppendCallback(() =>
                {
                    weaponSlot = WeaponSlot;
                    Debug.Log("Weapon Changed To: " + weaponSlot.GetComponent<WeaponSlotManager>().weapon);
                    WeaponSlotManager.ExpandInfoCard(weaponSlot);
                });
            }
            else
            {
                Debug.Log("a");
                DeselectWeapon(0); // If the same weapon slot is clicked, deselect it
            }
        }
        else
        {
            // Put the selected weapon into the currently selected slot
            selectedWeapon = WeaponSlot.GetComponent<WeaponSlotManager>().weapon; // Assign the selected weapon to the slot's manager
            shipSlot.GetComponent<Image>().sprite = selectedWeapon.weaponIcon; // Set the icon of the weapon in the slot
            //DataManager.SaveToArmory(shipSlot.transform.GetSiblingIndex(), selectedWeapon);
            SaveToPlayerArmory(selectedWeapon, shipSlot.transform.parent.GetSiblingIndex());
            Debug.Log(PlayerPrefs.GetString("PlayerArmory"));
            DeselectSlot();
            Debug.Log("b");
            DeselectWeapon(0); // Deselect the weapon slot after assigning the weapon
        }
    }

    private void SaveToPlayerArmory(Weapon weapon, int index)
    {
        Debug.Log(index);
        string[] currentArmory = PlayerPrefs.GetString("PlayerArmory").Split("|");
        string temp = "";
        for (int i=0;i<6;i++)
        {
            if (i!=index)
            {
                temp += currentArmory[i];
            }
            else
            {
                temp += weapon.weaponName;
            }
            if (i!=5)
            {
                temp += "|";
            }
            Debug.Log(temp);
        }
        PlayerPrefs.SetString("PlayerArmory", temp);
    }

    private Weapon GetWeaponByName(string name)
    {
        foreach (Weapon weapon in weaponList)
        {
            if (weapon.name == name)
            {
                return weapon;
            }
        }
        return null;
    }

    /// <summary>
    /// Starts the Coroutine to deselect weapon slot in the armory UI.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void DeselectWeapon(float delay = 0.1f)
    {
        StartCoroutine(DeselectWeaponCoroutine(delay));
    }

    /// <summary>
    /// Deselects the currently selected weapon slot in the armory UI.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    private IEnumerator DeselectWeaponCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay); // To prevent conflict with SelectWeapon method

        if (weaponSlot != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            WeaponSlotManager.ShrinkInfoCard(weaponSlot); // Shrink the info card of the weapon
            weaponSlot = null; // Clear the weapon slot variable
            Debug.Log("Weapon Deselected");
        }
    }
}
