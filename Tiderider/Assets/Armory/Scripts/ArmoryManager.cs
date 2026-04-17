using DG.Tweening;
using System;
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
    private GameObject shipSlot = null, weaponSlot = null;
    private Weapon selectedWeapon = null;
    [SerializeField] private Weapon[] weaponList = new Weapon[6];
    [SerializeField] private Transform slotContainer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadExistingArmory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadExistingArmory()
    {
        weaponList = DataManager.GetPlayerArmory();

        for (int i = 0; i < weaponList.Length; i++)
        {
            if (weaponList[i] == null) continue;

            if (slotContainer != null && i < slotContainer.childCount)
            {
                Transform slotTransform = slotContainer.GetChild(i).GetChild(0);
                Image slotImage = slotTransform.GetComponent<Image>();
                if (slotImage != null)
                {
                    slotImage.sprite = weaponList[i].weaponIcon;
                }
            }
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
        if (shipSlot != null)
        {
            if (weaponSlot != null)
            {
                WeaponSlotManager.ShrinkInfoCard(weaponSlot);
                weaponSlot = null;
            }

            if (shipSlot == Slot)
            {
                DeselectSlot();
                return;
            }
            else
            {
                DeselectSlot();
            }
        }

        shipSlot = Slot;
        shipSlot.GetComponent<Image>().color = shipSlot.GetComponent<Button>().colors.pressedColor;
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
        yield return new WaitForSeconds(delay);

        if (shipSlot != null)
        {
            shipSlot.GetComponent<Image>().color = Color.white;
            shipSlot = null;
        }
    }

    /// <summary>
    /// Selects a weapon for the currently selected slot in the armory UI.
    /// </summary>
    /// <param name="WeaponSlot">Weapon to be selected to the slot</param>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void SelectWeapon(GameObject WeaponSlot)
    {
        if (shipSlot == null)
        {
            if (weaponSlot == null)
            {
                weaponSlot = WeaponSlot;
                WeaponSlotManager.ExpandInfoCard(weaponSlot);
            }
            else if (weaponSlot != WeaponSlot)
            {
                DOTween.Sequence()
                .AppendCallback(() => WeaponSlotManager.ShrinkInfoCard(weaponSlot))
                .AppendInterval(0.5f)
                .AppendCallback(() =>
                {
                    weaponSlot = WeaponSlot;
                    WeaponSlotManager.ExpandInfoCard(weaponSlot);
                });
            }
            else
            {
                DeselectWeapon(0);
            }
        }
        else
        {
            int slotIndex = shipSlot.transform.parent.GetSiblingIndex();

            selectedWeapon = WeaponSlot.GetComponent<WeaponSlotManager>().weapon;
            shipSlot.GetComponent<Image>().sprite = selectedWeapon.weaponIcon;
            weaponList[slotIndex] = selectedWeapon;

            DataManager.SaveToArmory(slotIndex, selectedWeapon);

            DeselectSlot();
            DeselectWeapon(0);
        }
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
        yield return new WaitForSeconds(delay);

        if (weaponSlot != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            WeaponSlotManager.ShrinkInfoCard(weaponSlot);
            weaponSlot = null;
        }
    }

    public bool isArmoryComplete()
    {
        for (int i = 0; i < weaponList.Length; i++)
        {
            if (weaponList[i] == null)
            {
                return false;
            }
        }
        return true;
    }
}