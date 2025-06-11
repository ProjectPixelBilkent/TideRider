using UnityEngine;
using UnityEngine.UI; 


/// <summary>
/// Mother class for all weapons in the game.
/// </summary>
/// <remarks>
/// Created by: Kaan Aydınlı
/// </remarks>
public class Weapon : MonoBehaviour
{
    public string weaponName;
    public Color weaponColor = Color.white;
    public Sprite weaponIcon;

    public string weaponDescription;
    
    public int weaponDamage;


}