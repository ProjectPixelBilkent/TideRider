using UnityEngine;
using System.Collections.Generic;
using System;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Game Data")]
    public Weapon[] allAvailableWeapons;

    [Header("Weapon Frames")]
    public ShopItem[] weaponFrames = new ShopItem[4];

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

            frame.itemData = null; // IMPORTANT: weapon frames are NOT ScriptableObjects

            frame.iconImage.sprite = weapon.weaponIcon;
            frame.nameText.text = weapon.weaponName;

            if (level >= weapon.weaponLevelCount)
            {
                frame.levelOrDescText.text = "MAX LEVEL";
                frame.costText.text = "COMPLETED";
                frame.entireFrameButton.interactable = false;
            }
            else
            {
                frame.levelOrDescText.text = $"Lv. {level} → {level + 1}";
                frame.entireFrameButton.interactable = true;

                if (i == 0)
                {
                    bool ready = TimeManager.HasPassed(DataManager.GetLastWeaponAdTime(), weaponAdCooldown);
                    frame.costText.text = ready ? "FREE (AD)" : "COOLDOWN";
                }
                else
                {
                    int cost = weapon.weaponLevels[level].cost;
                    frame.costText.text = $"{cost} COINS";
                }

                // Assign click behavior dynamically
                frame.entireFrameButton.onClick.RemoveAllListeners();
                frame.entireFrameButton.onClick.AddListener(() =>
                {
                    HandleWeaponPurchase(i, weaponIdx, weapon, level);
                });
            }
        }
    }

    private void HandleWeaponPurchase(int frameIndex, int weaponIdx, Weapon weapon, int level)
    {
        if (frameIndex == 0)
        {
            if (!TimeManager.HasPassed(DataManager.GetLastWeaponAdTime(), weaponAdCooldown))
            {
                NotificationManager.Instance.ShowNotification("Ad not ready yet!");
                return;
            }

            AdManager.Instance.ShowRewardedAd(() => {
                ResourceManager.Instance.UpgradeWeaponDirectly(weaponIdx);
                DataManager.SetLastWeaponAdTime(TimeManager.GetCurrentTimeString());
                UpdateWeaponShopUI();
            });
        }
        else
        {
            ResourceManager.UpgradeWeapon(weapon);
        }

        UpdateWeaponShopUI();
    }

    public void ProcessPurchase(ShopItem item)
    {
        item.itemData.Purchase();
    }

    public void UnlockNoAds()
    {
        DataManager.UnlockNoAds();
        Debug.Log("IAP Bridge: No Ads Unlocked!");
    }
}