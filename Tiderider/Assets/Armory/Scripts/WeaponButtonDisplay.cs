using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image), typeof(Weapon))]

/// <summary>
/// Displays the weapon icon and color on a button in the armory UI.
/// </summary>
/// <remarks>
/// Created by: Kaan Aydınlı
/// </remarks>
public class WeaponButtonDisplay : MonoBehaviour
{

    void Awake()
    {

        Weapon weaponData = GetComponent<Weapon>();
        Image buttonImage = GetComponent<Image>();

        // Check if the weaponData and buttonImage are not null
        if (weaponData.weaponIcon != null)
        {

            buttonImage.sprite = weaponData.weaponIcon;
        }
        else
        {
            
            buttonImage.sprite = null;
            buttonImage.color = weaponData.weaponColor;
        }
    }
}