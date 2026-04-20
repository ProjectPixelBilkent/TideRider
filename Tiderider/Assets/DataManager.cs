using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
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
    public static void SubtractCoinAmount(int amount = 1)
    {
        GameData gameData = LoadGameData();
        gameData.coinAmount -= amount;
        SaveGameData(gameData);
    }

    /// <summary>
    /// Increments the energy amount in game data.
    /// </summary>
    /// <remarks>
    /// Created by: Işık Dönger
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void IncrementEnergyAmount(int amount = 1)
    {
        GameData gameData = LoadGameData();
        gameData.energyAmount += amount;
        SaveGameData(gameData);
    }

    /// <summary>
    /// Substracts from the energy amount in game data.
    /// </summary>
    /// <remarks>
    /// Created by: Işık Dönger
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void DecrementEnergyAmount()
    {
        GameData gameData = LoadGameData();
        gameData.energyAmount -= 1;

        if (string.IsNullOrEmpty(gameData.lastEnergyUpdateTime))
        {
            gameData.lastEnergyUpdateTime = DateTime.Now.ToString();
        }

        SaveGameData(gameData);
    }

    /// <summary>
    /// Updates the highest level unlocked only if the player completes their current furthest level.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void CompleteLevel(int completedLevelIndex)
    {
        GameData gameData = LoadGameData();

        if (completedLevelIndex == gameData.highestUnlockedLevelIndex)
        {
            gameData.highestUnlockedLevelIndex++;
            SaveGameData(gameData);
        }
    }

    /// <summary>
    /// Adds the conversation to the completed list and saves the game data.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void MarkConversationCompleted(string conversationId)
    {
        GameData gameData = LoadGameData();

        if (gameData.completedConversations == null)
        {
            gameData.completedConversations = new List<string>();
        }

        if (!gameData.completedConversations.Contains(conversationId))
        {
            gameData.completedConversations.Add(conversationId);
            SaveGameData(gameData);
        }
    }

    /// <summary>
    /// Saves the selected weapon in player armory in game data.
    /// </summary>
    /// <remarks>
    /// Created by: Ata Uzay Kuzey
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void SaveToArmory(int index, Weapon weapon)
    {
        GameData gameData = LoadGameData();
        gameData.playerArmory[index] = weapon;
        SaveGameData(gameData);
    }

    public static void SetDailyShopData(string resetTime, List<int> weaponIndices)
    {
        GameData data = LoadGameData();
        data.lastDailyResetTime = resetTime;
        data.dailyShopWeaponIndices = weaponIndices;
        SaveGameData(data);
    }

    public static void SetLastEnergyAdTime(string time)
    {
        GameData data = LoadGameData();
        data.lastEnergyAdTime = time;
        SaveGameData(data);
    }

    public static void SetLastWeaponAdTime(string time)
    {
        GameData data = LoadGameData();
        data.lastWeaponAdTime = time;
        SaveGameData(data);
    }

    public static void UnlockNoAds()
    {
        GameData gameData = LoadGameData();
        gameData.hasRemovedAds = true;
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
    /// Loads the energy amount from the local backup.
    /// </summary>
    /// <returns>Coin Amount.</returns>
    /// <remarks>
    /// Created by: Işık Dönger
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetEnergyAmount() => LoadGameData().energyAmount;

    /// <summary>
    /// Loads the current highest unlocked level index from the local backup.
    /// </summary>
    /// <returns>Highest Unlocked Level Index.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetHighestUnlockedIndex() => LoadGameData().highestUnlockedLevelIndex;

    /// <summary>
    /// Checks if a conversation has already been shown based on the local backup.
    /// </summary>
    /// <returns>True if completed, false otherwise.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static bool IsConversationCompleted(string conversationId)
    {
        GameData gameData = LoadGameData();

        if (gameData.completedConversations == null)
            return false;

        return gameData.completedConversations.Contains(conversationId);
    }

    public static string GetLastDailyResetTime() => LoadGameData().lastDailyResetTime;
    public static string GetLastEnergyAdTime() => LoadGameData().lastEnergyAdTime;
    public static string GetLastWeaponAdTime() => LoadGameData().lastWeaponAdTime;
    public static List<int> GetDailyShopWeapons() => LoadGameData().dailyShopWeaponIndices;

    /// <summary>
    /// Loads the player armory from the local backup.
    /// </summary>
    /// <returns>Player Armory.</returns>
    /// <remarks>
    /// Created by: Ata Uzay Kuzey
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static Weapon[] GetPlayerArmory() => LoadGameData().playerArmory;

    public static bool HasRemovedAds() => LoadGameData().hasRemovedAds;

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
