using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnergyItem", menuName = "Scriptable Objects/Shop/Energy Item")]
public class EnergyItemData : ShopItemData
{
    public int amountToGive;
    public int coinCost;

    public override void Purchase()
    {
        if (isAdReward)
        {
            if (DataManager.HasRemovedAds())
            {
                ResourceManager.Instance.AddEnergy(amountToGive);
                return;
            }

            if (!TimeManager.HasPassed(DataManager.GetLastEnergyAdTime(), TimeSpan.FromHours(1)))
            {
                NotificationManager.Instance.ShowNotification("Ad not ready yet!");
                return;
            }

            AdManager.Instance.ShowRewardedAd(() => {
                ResourceManager.Instance.AddEnergy(amountToGive);
                DataManager.SetLastEnergyAdTime(TimeManager.GetCurrentTimeString());
            });
        }
        else
        {
            if (!ResourceManager.Instance.TryBuyWithCoins(coinCost))
                return;

            ResourceManager.Instance.AddEnergy(amountToGive);
        }
    }
}