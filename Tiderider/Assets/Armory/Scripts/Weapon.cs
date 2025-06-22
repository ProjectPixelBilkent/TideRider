using UnityEngine;
using System;
using Unity.VisualScripting;

/// <summary>
/// Serializable class representing a level of a weapon with its properties.
/// </summary>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
[Serializable]
public class WeaponLevel
{
    public int cost; // Cost to upgrade to this level
    public float damage;
    public float fireRate; // Time between shots in seconds
    public float range; // Maximum range of the weapon in Unity distance units
    public float HP; // Health points of the weapon
    public float duration; // Duration for which the weapon can be used (e.g., for flamethrowers)
    public GameObject projectilePrefab; // Prefab for the projectile fired by the weapon
}

/// <summary>
/// ScriptableObject representing a weapon in the game.
/// </summary>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class Weapon : ScriptableObject
{
    public string weaponName;
    public WeaponLevel[] weaponLevels; // Array of levels for the weapon
    public Sprite weaponIcon;
}