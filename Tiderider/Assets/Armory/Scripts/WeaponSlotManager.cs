using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;

/// <summary>
/// MonoBehaviour class for managing the weapon slots in the game.
/// </summary>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
public class WeaponSlotManager : MonoBehaviour
{
    public Weapon weapon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<Image>().sprite = weapon.weaponIcon; // Set the icon of the weapon in the slot
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
}
