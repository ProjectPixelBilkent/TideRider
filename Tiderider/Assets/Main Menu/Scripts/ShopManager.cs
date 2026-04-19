using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Game Data")]
    public Weapon[] allAvailableWeapons;

    [Header("Weapon Frames")]
    public ShopItem[] weaponFrames = new ShopItem[4];

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
        UpdateWeaponShopUI();
    }

    private void CheckDailyReset()
    {
        string lastReset = DataManager.GetLastDailyResetTime();
        if (TimeManager.HasPassed(lastReset, dailyResetCooldown) || DataManager.GetDailyShopWeapons().Count == 0)
        {
            List<int> newDailyWeapons = GenerateRandomDailyWeapons(4);
            DataManager.SetDailyShopData(TimeManager.GetCurrentTimeString(), newDailyWeapons);
        }
    }

    private List<int> GenerateRandomDailyWeapons(int count)
    {
        List<int> available = new List<int>();
        for (int i = 0; i < allAvailableWeapons.Length; i++) available.Add(i);
        List<int> dailyPicks = new List<int>();
        for (int i = 0; i < count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, available.Count);
            dailyPicks.Add(available[randomIndex]);
            available.RemoveAt(randomIndex);
        }
        return dailyPicks;
    }

    public void UpdateWeaponShopUI()
    {
        List<int> dailyWeaponIndices = DataManager.GetDailyShopWeapons();
        List<int> currentLevels = DataManager.GetWeaponLevels();

        for (int i = 0; i < weaponFrames.Length; i++)
        {
            if (i >= dailyWeaponIndices.Count) break;

            int weaponIdx = dailyWeaponIndices[i];
            Weapon weapon = allAvailableWeapons[weaponIdx];
            int level = currentLevels[weaponIdx];
            ShopItem frame = weaponFrames[i];

            if (frame.iconImage != null) frame.iconImage.sprite = weapon.weaponIcon;
            if (frame.nameText != null) frame.nameText.text = weapon.weaponName;

            if (level >= weapon.weaponLevelCount)
            {
                if (frame.levelOrDescText != null) frame.levelOrDescText.text = "MAX LEVEL";
                if (frame.costText != null) frame.costText.text = "COMPLETED";
                if (frame.entireFrameButton != null) frame.entireFrameButton.interactable = false;
            }
            else
            {
                if (frame.levelOrDescText != null) frame.levelOrDescText.text = $"Lv. {level} -> {level + 1}";
                if (frame.entireFrameButton != null) frame.entireFrameButton.interactable = true;

                if (i == 0)
                {
                    bool ready = TimeManager.HasPassed(DataManager.GetLastWeaponAdTime(), weaponAdCooldown);
                    if (frame.costText != null) frame.costText.text = ready ? "FREE (AD)" : "COOLDOWN";
                }
                else
                {
                    if (frame.costText != null) frame.costText.text = $"{weapon.weaponLevels[level].cost} COINS";
                }
            }
        }
    }

    public void ProcessPurchase(ShopItem item)
    {
        ShopItemData.ItemType currentType = item.itemData != null ? item.itemData.type : ShopItemData.ItemType.Weapon;

        switch (currentType)
        {
            case ShopItemData.ItemType.Weapon:
                if (item.frameIndex == 0) OnUpgradeWeaponWithAd(item.frameIndex);
                else OnUpgradeWeaponWithCoins(item.frameIndex);
                break;

            case ShopItemData.ItemType.Energy:
                if (item.itemData.isAdReward) OnBuyEnergyWithAd();
                else OnBuyEnergyWithCoins(item.itemData.coinCost, item.itemData.amountToGive);
                break;

            case ShopItemData.ItemType.Coin:
                NotificationManager.Instance.ShowNotification("Opening Store...");
                break;
        }
    }

    private void OnUpgradeWeaponWithAd(int frameIndex)
    {
        if (TimeManager.HasPassed(DataManager.GetLastWeaponAdTime(), weaponAdCooldown))
        {
            int weaponIdx = DataManager.GetDailyShopWeapons()[frameIndex];
            ResourceManager.Instance.UpgradeWeaponDirectly(weaponIdx);
            DataManager.SetLastWeaponAdTime(TimeManager.GetCurrentTimeString());
            UpdateWeaponShopUI();
        }
        else
        {
            string remain = TimeManager.GetRemainingTimeFormatted(DataManager.GetLastWeaponAdTime(), weaponAdCooldown);
            NotificationManager.Instance.ShowNotification($"Ad ready in: {remain}");
        }
    }

    private void OnUpgradeWeaponWithCoins(int frameIndex)
    {
        int weaponIdx = DataManager.GetDailyShopWeapons()[frameIndex];
        int level = DataManager.GetWeaponLevels()[weaponIdx];
        int cost = allAvailableWeapons[weaponIdx].weaponLevels[level].cost;

        if (ResourceManager.Instance.TryBuyWithCoins(cost))
        {
            ResourceManager.Instance.UpgradeWeaponDirectly(weaponIdx);
            UpdateWeaponShopUI();
        }
    }

    private void OnBuyEnergyWithAd()
    {
        if (TimeManager.HasPassed(DataManager.GetLastEnergyAdTime(), energyAdCooldown))
        {
            ResourceManager.Instance.AddEnergy(1);
            DataManager.SetLastEnergyAdTime(TimeManager.GetCurrentTimeString());
            ResourceManager.Instance.UpdateUI();
        }
        else
        {
            string remain = TimeManager.GetRemainingTimeFormatted(DataManager.GetLastEnergyAdTime(), energyAdCooldown);
            NotificationManager.Instance.ShowNotification($"Ad ready in: {remain}");
        }
    }

    private void OnBuyEnergyWithCoins(int cost, int amount)
    {
        if (ResourceManager.Instance.TryBuyWithCoins(cost))
        {
            ResourceManager.Instance.AddEnergy(amount);
        }
    }
}