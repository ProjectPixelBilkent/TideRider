using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using static LocalBackupManager;

public static class DataManager
{
    /// <summary>
    /// Increments the coin amount in game data.
    /// </summary>
    /// <remarks>
    /// Created by: Ata Uzay Kuzey
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void IncrementCoinAmount(int amount)
    {
        GameData gameData = LoadGameData();
        gameData.coinAmount += amount;
        SaveGameData(gameData);
    }

    /// <summary>
    /// Substracts from the coin amount in game data.
    /// </summary>
    /// <remarks>
    /// Created by: Ata Uzay Kuzey
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void SubtractCoinAmount(int amount)
    {
        GameData gameData = LoadGameData();
        gameData.coinAmount -= amount;
        SaveGameData(gameData);
    }

    /// <summary>
    /// Increments the canon level in weapon data.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void IncrementCanonLevel()
    {
        WeaponData weaponData = LoadWeaponData();
        weaponData.canonLevel++;
        SaveWeaponData(weaponData);
    }

    /// <summary>
    /// Increments the minigun level in weapon data.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void IncrementMinigunLevel()
    {
        WeaponData weaponData = LoadWeaponData();
        weaponData.minigunLevel++;
        SaveWeaponData(weaponData);
    }

    /// <summary>
    /// Increments the shield level in weapon data.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void IncrementShieldLevel()
    {
        WeaponData weaponData = LoadWeaponData();
        weaponData.shieldLevel++;
        SaveWeaponData(weaponData);
    }

    /// <summary>
    /// Increments the lasergun level in weapon data.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void IncrementLasergunLevel()
    {
        WeaponData weaponData = LoadWeaponData();
        weaponData.lasergunLevel++;
        SaveWeaponData(weaponData);
    }

    /// <summary>
    /// Increments the flamethrower level in weapon data.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void IncrementFlamethrowerLevel()
    {
        WeaponData weaponData = LoadWeaponData();
        weaponData.flamethrowerLevel++;
        SaveWeaponData(weaponData);
    }

    /// <summary>
    /// Increments the icegun level in weapon data.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void IncrementIcegunLevel()
    {
        WeaponData weaponData = LoadWeaponData();
        weaponData.icegunLevel++;
        SaveWeaponData(weaponData);
    }

    /// <summary>
    /// Loads the coin amount from the local backup.
    /// </summary>
    /// <returns>Coin Amount.</returns>
    /// <remarks>
    /// Created by: Ata Uzay Kuzey
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetCoinAmount() => LoadGameData().coinAmount;

    /// <summary>
    /// Loads the current canon level from the local backup.
    /// </summary>
    /// <returns>Current Canon Level.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetCanonLevel() => LoadWeaponData().canonLevel;

    /// <summary>
    /// Loads the current minigun level from the local backup.
    /// </summary>
    /// <returns>Current Minigun Level.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetMinigunLevel() => LoadWeaponData().minigunLevel;

    /// <summary>
    /// Loads the current shield level from the local backup.
    /// </summary>
    /// <returns>Current Shield Level.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetShieldLevel() => LoadWeaponData().shieldLevel;

    /// <summary>
    /// Loads the current laser level from the local backup.
    /// </summary>
    /// <returns>Current Laser Level.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetLasergunLevel() => LoadWeaponData().lasergunLevel;

    /// <summary>
    /// Loads the current flamethrower level from the local backup.
    /// </summary>
    /// <returns>Current Flamethrower Level.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetFlamethrowerLevel() => LoadWeaponData().flamethrowerLevel;

    /// <summary>
    /// Loads the current icegun level from the local backup.
    /// </summary>
    /// <returns>Current Icegun Level.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetIcegunLevel() => LoadWeaponData().icegunLevel;

    /// <summary>
    /// Loads the current level of all weapons from the local backup.
    /// </summary>
    /// <returns>Current Weapon Level List.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static List<int> GetWeaponLevels()
    {
        return new List<int>
        {
            GetCanonLevel(),
            GetMinigunLevel(),
            GetShieldLevel(),
            GetLasergunLevel(),
            GetFlamethrowerLevel(),
            GetIcegunLevel()
        };
    }
}
