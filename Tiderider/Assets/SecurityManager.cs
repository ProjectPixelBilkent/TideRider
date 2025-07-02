using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;
using UnityEngine;

/// <summary>
/// Static class for handling data security.
/// </summary>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
public static class SecurityManager
{
    /// <summary>
    /// Saves encrypted data to a file and stores its hash for integrity checking.
    /// </summary>
    /// <param name="filename">The name of the file to save to.</param>
    /// <param name="data">The plaintext data to encrypt and save.</param>
    /// <param name="encryptionKey">32-byte encryption key for AES.</param>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static void SaveEncryptedData(string filename, string data, byte[] encryptionKey)
    {
        // Validate encryption key
        if (encryptionKey == null || encryptionKey.Length != 32)
        {
            Debug.LogWarning("Encryption key is null or empty!");
            return;
        }

        try
        {
            // Encrypt data and compute hash
            string encryptedData = Encrypt(data, encryptionKey);
            string hash = ComputeSHA256(encryptedData);

            // Combine encrypted data with hash and save to file
            string filePath = Path.Combine(Application.persistentDataPath, filename);
            File.WriteAllText(filePath, encryptedData + "\n" + hash);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error saving encrypted data: " + ex.Message);
        }
    }

    /// <summary>
    /// Loads and decrypts data, verifying integrity with SHA-256.
    /// </summary>
    /// <param name="filename">The name of the file to load from.</param>
    /// <param name="encryptionKey">32-byte encryption key for AES.</param>
    /// <returns>Decrypted data if successful, null otherwise.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static string LoadEncryptedData(string filename, byte[] encryptionKey)
    {
        // Validate encryption key
        if (encryptionKey == null || encryptionKey.Length != 32)
        {
            Debug.LogWarning("Encryption key is null or empty!");
            return null;
        }

        string filePath = Path.Combine(Application.persistentDataPath, filename);
        if (!File.Exists(filePath)) return null;

        try
        {
            // Read file contents
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length < 2) return null; // Invalid file format

            string encryptedData = lines[0];
            string storedHash = lines[1];

            // Verify data integrity
            string computedHash = ComputeSHA256(encryptedData);
            if (storedHash != computedHash)
            {
                Debug.LogWarning("Data integrity check failed! The file might be corrupted.");
                return null;
            }

            return Decrypt(encryptedData, encryptionKey);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error loading encrypted data: " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Encrypts text using AES with a random IV for each encryption.
    /// </summary>
    /// <param name="plainText">Text to encrypt.</param>
    /// <param name="encryptionKey">32-byte encryption key.</param>
    /// <returns>Base64-encoded encrypted data with IV prepended.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static string Encrypt(string plainText, byte[] encryptionKey)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = encryptionKey;
            aes.GenerateIV(); // Generate random IV for each encryption

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var memoryStream = new MemoryStream())
            {
                // Write IV first
                memoryStream.Write(aes.IV, 0, aes.IV.Length);

                // Encrypt the data
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                    cryptoStream.FlushFinalBlock();
                }

                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }

    /// <summary>
    /// Decrypts text using AES.
    /// </summary>
    /// <param name="encryptedText">Base64-encoded encrypted data with IV.</param>
    /// <param name="encryptionKey">32-byte encryption key.</param>
    /// <returns>Decrypted plaintext.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static string Decrypt(string encryptedText, byte[] encryptionKey)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

        using (Aes aes = Aes.Create())
        {
            aes.Key = encryptionKey;

            // Extract IV from beginning of encrypted data
            byte[] iv = new byte[16];
            Array.Copy(encryptedBytes, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var memoryStream = new MemoryStream(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            using (var streamReader = new StreamReader(cryptoStream))
            {
                return streamReader.ReadToEnd();
            }
        }
    }

    /// <summary>
    /// Computes the SHA-256 hash of a given input string.
    /// </summary>
    /// <param name="input">String to hash.</param>
    /// <returns>Base64-encoded hash value.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    private static string ComputeSHA256(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
