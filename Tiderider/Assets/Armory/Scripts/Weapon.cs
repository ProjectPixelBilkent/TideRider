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
    public int damage;
    public float fireRate; // Time between shots in seconds
    public float speedOfBullet; //How fast the bullet is
    public float range; // Maximum range of the weapon in Unity distance units
    public float HP; // Health points of the weapon
    public float duration; // Duration for which the weapon can be used (e.g., for flamethrowers)
}

[Serializable]
public struct WeaponStat
{
    public Weapon weaponInfo;
    public int level;

    public WeaponLevel WeaponLevel { get { return weaponInfo.weaponLevels[level]; } }

    public WeaponStat(Weapon weaponInfo, int level)
    {
        this.weaponInfo = weaponInfo;
        this.level = level;
    }
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
    public static Vector3[] BulletOffsets = new Vector3[] { new Vector3(-0.707f, 0.707f, 0), new Vector3(0.707f, 0.707f, 0), new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(-0.707f, -0.707f, 0), new Vector3(0.707f, -0.707f, 0) };
    public static Vector3[] BulletDirections = new Vector3[] { new Vector3(-1, 1, 0), new Vector3(1, 1, 0), new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(-1, -1, 0), new Vector3(1, -1, 0) };

    public string weaponName;
    public string weaponDescription; // Description of the weapon
    public WeaponLevel[] weaponLevels; // Array of levels for the weapon
    public int weaponLevelCount => weaponLevels.Length; // Total number of levels for the weapon
    public Sprite weaponIcon;
    public GameObject projectilePrefab; // Prefab for the projectile fired by the weapon
    [SerializeField] private string onCollisionWithBulletMethodName;

    public void OnCollisionWithBullet(ShipModel model, int level, Bullet bullet)
    {
        GetType().GetMethod(onCollisionWithBulletMethodName).Invoke(this, new object[] { model, level, bullet });
    }

    public void NormalBullet(ShipModel model, int level, Bullet bullet)
    {
        model.Decrement(weaponLevels[level].damage);
    }

    public void MiniShipBullet(ShipModel model, int level, Bullet bullet) { }
}