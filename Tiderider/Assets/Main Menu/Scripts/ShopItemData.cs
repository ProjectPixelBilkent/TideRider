using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Scriptable Objects/Shop Item Data")]
public class ShopItemData : ScriptableObject
{
    public enum ItemType { Weapon, Energy, Coin }

    public ItemType type;
    public string itemName;
    public Sprite itemIcon;

    [Header("Values")]
    public int coinCost;
    public int amountToGive;

    [Tooltip("Ad Checkmark")]
    public bool isAdReward;
}