using System;
using UnityEngine;

public abstract class ShopItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public bool isAdReward;

    public abstract void Purchase();
}

[CreateAssetMenu(fileName = "EnergyItem", menuName = "Scriptable Objects/Shop/Energy Item")]
public class EnergyItemData : ShopItemData
{
    public int amountToGive;
    public int coinCost;

    public override void Purchase()
    {
        if (isAdReward)
        {
            if (!TimeManager.HasPassed(DataManager.GetLastEnergyAdTime(), TimeSpan.FromHours(1)))
            {
                NotificationManager.Instance.ShowNotification("Ad not ready yet!");
                return;
            }

            ResourceManager.Instance.AddEnergy(amountToGive);
            DataManager.SetLastEnergyAdTime(TimeManager.GetCurrentTimeString());
        }
        else
        {
            if (!ResourceManager.Instance.TryBuyWithCoins(coinCost))
                return;

            ResourceManager.Instance.AddEnergy(amountToGive);
        }
    }
}

[CreateAssetMenu(fileName = "CoinItem", menuName = "Scriptable Objects/Shop/Coin Item")]
public class CoinItemData : ShopItemData
{
    public int amountToGive;
    public string productId;

    public override void Purchase()
    {
        // TODO: IAP
        ResourceManager.Instance.AddCoins(amountToGive);
    }
}