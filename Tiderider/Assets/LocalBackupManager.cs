using UnityEngine;
using System.Collections.Generic;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using System.IO;

/// <summary>
/// Static class for managing local backups.
/// </summary>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
public static class LocalBackupManager
{
    private const string BackupDirectory = "Backups";
    private const string WeaponDataFile = "weaponData.dat";
    private const string GameDataFile = "gameData.dat";

    /// <summary>
    /// Static method to save game data to a local backup file.
    /// </summary>
    /// <param name="data"></param>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void SaveGameData(GameData data)
    {
        string backupPath = Path.Combine(Application.persistentDataPath, BackupDirectory);

        if (!Directory.Exists(backupPath))
        {
            Directory.CreateDirectory(backupPath);
        }

        string filePath = Path.Combine(backupPath, GameDataFile);

        byte[] encryptionKey = GenerateEncryptionKey();
        if (encryptionKey == null || encryptionKey.Length != 32)
        {
            Debug.LogWarning("Encryption key is null or empty!");
            return;
        }

        string jsonData = JsonUtility.ToJson(data);
        SecurityManager.SaveEncryptedData(filePath, jsonData, encryptionKey);
    }

    /// <summary>
    /// Static method to save weapon data to a local backup file.
    /// </summary>
    /// <param name="data"></param>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void SaveWeaponData(WeaponData data)
    {
        string backupPath = Path.Combine(Application.persistentDataPath, BackupDirectory);
        if (!Directory.Exists(backupPath))
        {
            Directory.CreateDirectory(backupPath);
        }
        string filePath = Path.Combine(backupPath, WeaponDataFile);

        byte[] encryptionKey = GenerateEncryptionKey();
        if (encryptionKey == null || encryptionKey.Length != 32)
        {
            Debug.LogWarning("Encryption key is null or empty!");
            return;
        }

        string jsonData = JsonUtility.ToJson(data);
        SecurityManager.SaveEncryptedData(filePath, jsonData, encryptionKey);
    }

    /// <summary>
    /// Static method to load game data from a local backup file.
    /// </summary>
    /// <returns>
    /// 
    /// Data.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static GameData LoadGameData()
    {
        string backupPath = Path.Combine(Application.persistentDataPath, BackupDirectory);

        if (!Directory.Exists(backupPath))
        {
            Directory.CreateDirectory(backupPath);
        }

        string filePath = Path.Combine(backupPath, GameDataFile);

        if (!File.Exists(filePath)) return new GameData();

        byte[] encryptionKey = GenerateEncryptionKey();
        if (encryptionKey == null || encryptionKey.Length != 32)
        {
            Debug.LogWarning("Encryption key is null or empty!");
            return new GameData();
        }

        string jsonData = SecurityManager.LoadEncryptedData(filePath, encryptionKey);
        if (string.IsNullOrEmpty(jsonData))
        {
            Debug.LogWarning("Failed to load weapon data.");
            return new GameData();
        }

        return JsonUtility.FromJson<GameData>(jsonData);
    }

    /// <summary>
    /// Static method to load weapon data from a local backup file.
    /// </summary>
    /// <returns>WeaponData.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static WeaponData LoadWeaponData()
    {
        string backupPath = Path.Combine(Application.persistentDataPath, BackupDirectory);

        if (Directory.Exists(backupPath))
        {
            Directory.CreateDirectory(backupPath);
        }

        string filePath = Path.Combine(backupPath, WeaponDataFile);

        if (!File.Exists(filePath)) return new WeaponData();

        byte[] encryptionKey = GenerateEncryptionKey();
        if (encryptionKey == null || encryptionKey.Length != 32)
        {
            Debug.LogWarning("Encryption key is null or empty!");
            return new WeaponData();
        }

        string jsonData = SecurityManager.LoadEncryptedData(filePath, encryptionKey);
        if (string.IsNullOrEmpty(jsonData))
        {
            Debug.LogWarning("Failed to load weapon data.");
            return new WeaponData();
        }

       return JsonUtility.FromJson<WeaponData>(jsonData);
    }

    /// <summary>
    /// Generates an encryption key using PBKDF2.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    private static byte[] GenerateEncryptionKey()
    {
        string saltBase64 = "DxiwESUup/kWsraK90A62pkHpWqnRdOv+LyjA4YPwWU=";
        byte[] salt = Encoding.UTF8.GetBytes(saltBase64);
        using (var deriveBytes = new Rfc2898DeriveBytes(
            password: "asdnj21l2312",
            salt: salt,
            iterations: 600000, // Increased for security
            hashAlgorithm: HashAlgorithmName.SHA256))
        {
            return deriveBytes.GetBytes(32); // 32 bytes = 256-bit key
        }
    }
}
