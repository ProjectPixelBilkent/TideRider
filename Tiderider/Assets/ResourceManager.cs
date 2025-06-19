using UnityEngine;

/// <summary>
/// Static class for managing game resources.
/// </summary>
/// <remarks>
/// Created by: Ata Uzay Kuzey
/// </remarks>
public static class ResourceManager
{
    private const string COIN_KEY = "CoinAmount";

    private static int coinAmount;
    private static bool initialized = false;

    /// <summary>
    /// Ensures that the coinAmount is loaded from PlayerPrefs.
    /// If no value exists yet, defaults to 0.
    /// </summary>
    /// <remarks>
    /// Maintained by: Ata Uzay Kuzey
    /// </remarks>
    public static void Initialize()
    {
        if (initialized) 
        {
            return;
        }

        coinAmount = PlayerPrefs.GetInt(COIN_KEY, 0);
        initialized = true;
    }

    /// <summary>
    /// Returns the current coin amount, loading from memory if necessary.
    /// </summary>
    /// <returns>Current number of coins.
    /// <remarks>
    /// Maintained by: Ata Uzay Kuzey
    /// </remarks>
    public static int GetCoin()
    {
        Initialize();
        return coinAmount;
    }

    /// <summary>
    /// Sets the coin amount, writes it to disk immediately.
    /// </summary>
    /// <paramref name="amount"/>The coin amount to be set.</param>
    /// <remarks>
    /// Maintained by: Ata Uzay Kuzey
    /// </remarks>
    public static void SetCoin(int amount)
    {
        Initialize();
        coinAmount = amount;
        PlayerPrefs.SetInt(COIN_KEY, coinAmount);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Convenience method to add coins (can be negative to subtract).
    /// </summary>
    /// <paramref name="delta"/>The difference in coin amount.</param>
    /// <remarks>
    /// Maintained by: Ata Uzay Kuzey
    /// </remarks>
    public static void AddCoins(int delta)
    { 
        SetCoin(GetCoin() + delta);
    }
}