using System;
using UnityEngine;

public abstract class ShopItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public bool isAdReward;

    public abstract void Purchase();
}