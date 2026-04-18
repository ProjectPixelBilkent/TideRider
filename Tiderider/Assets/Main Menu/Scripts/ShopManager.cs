using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Shop Settings")]
    public int totalWeaponsInGame = 6;

    // Define the cooldowns
    private readonly TimeSpan energyAdCooldown = TimeSpan.FromHours(1);
    private readonly TimeSpan weaponAdCooldown = TimeSpan.FromDays(1);
    private readonly TimeSpan dailyResetCooldown = TimeSpan.FromDays(1);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        CheckDailyReset();
    }

    /// <summary>
    /// Checks if a day has passed. If so, rolls new weapons for the shop.
    /// </summary>
    private void CheckDailyReset()
    {
        string lastReset = DataManager.GetLastDailyResetTime();

        if (TimeManager.HasPassed(lastReset, dailyResetCooldown))
        {
            List<int> newDailyWeapons = GenerateRandomDailyWeapons(4);
            DataManager.SetDailyShopData(TimeManager.GetCurrentTimeString(), newDailyWeapons);
            Debug.Log("Shop weapons rolled for the day!");
        }
    }

    private List<int> GenerateRandomDailyWeapons(int count)
    {
        List<int> available = new List<int>();
        for (int i = 0; i < totalWeaponsInGame; i++) available.Add(i);

        List<int> dailyPicks = new List<int>();

        // Shuffle and pick
        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, available.Count);
            dailyPicks.Add(available[randomIndex]);
            available.RemoveAt(randomIndex); // Ensure no duplicates
        }
        return dailyPicks;
    }

    // ==========================================
    // ENERGY ROW (4 Frames)
    // ==========================================

    public void OnBuyEnergyWithAd()
    {
        if (TimeManager.HasPassed(DataManager.GetLastEnergyAdTime(), energyAdCooldown))
        {
            // TODO: Call your Ad Network here. Example: AdManager.ShowRewarded(() => { ... })
            // On Ad Success callback:

            ResourceManager.Instance.AddEnergy(1); // Give reward
            DataManager.SetLastEnergyAdTime(TimeManager.GetCurrentTimeString()); // Reset timer
        }
        else
        {
            string remain = TimeManager.GetRemainingTimeFormatted(DataManager.GetLastEnergyAdTime(), energyAdCooldown);
            Debug.Log($"Ad not ready. Wait: {remain}");
            NotificationManager.Instance.ShowNotification($"Wait {remain}");
        }
    }

    public void OnBuyEnergyWithCoins(int coinCost, int energyAmount)
    {
        if (ResourceManager.Instance.TryBuyWithCoins(coinCost))
        {
            ResourceManager.Instance.AddEnergy(energyAmount);
        }
    }

    public void OnBuyEnergyWithIAP(int energyAmount)
    {
        // TODO: Hook up to Unity IAP
        // On Purchase Success:
        // ResourceManager.Instance.AddEnergy(energyAmount);
    }

    // ==========================================
    // COIN ROW (4 Frames)
    // ==========================================

    public void OnBuyCoinsWithIAP(int coinAmount)
    {
        // TODO: Hook up to Unity IAP
        // On Purchase Success:
        // ResourceManager.Instance.AddCoins(coinAmount);
    }

    // ==========================================
    // WEAPON UPGRADE ROW (4 Frames)
    // ==========================================

    public void OnUpgradeWeaponWithAd(int frameIndex)
    {
        if (TimeManager.HasPassed(DataManager.GetLastWeaponAdTime(), weaponAdCooldown))
        {
            // TODO: Call your Ad Network here.
            // On Ad Success callback:

            int weaponIndexToUpgrade = DataManager.GetDailyShopWeapons()[frameIndex];
            ResourceManager.Instance.UpgradeWeaponDirectly(weaponIndexToUpgrade);
            DataManager.SetLastWeaponAdTime(TimeManager.GetCurrentTimeString());
        }
        else
        {
            string remain = TimeManager.GetRemainingTimeFormatted(DataManager.GetLastWeaponAdTime(), weaponAdCooldown);
            Debug.Log($"Ad not ready. Wait: {remain}");
        }
    }

    public void OnUpgradeWeaponWithCoins(int frameIndex, int coinCost)
    {
        if (ResourceManager.Instance.TryBuyWithCoins(coinCost))
        {
            int weaponIndexToUpgrade = DataManager.GetDailyShopWeapons()[frameIndex];
            ResourceManager.Instance.UpgradeWeaponDirectly(weaponIndexToUpgrade);
        }
    }

    public void OnUpgradeWeaponWithIAP(int frameIndex)
    {
        // TODO: Hook up to Unity IAP
        // On Purchase Success:
        // int weaponIndexToUpgrade = DataManager.GetDailyShopWeapons()[frameIndex];
        // ResourceManager.Instance.UpgradeWeaponDirectly(weaponIndexToUpgrade);
    }
}