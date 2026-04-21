using UnityEngine;

[CreateAssetMenu(fileName = "CoinItem", menuName = "Scriptable Objects/Shop/Coin Item")]
public class CoinItemData : ShopItemData
{
    public int amountToGive;
    public string productId;

    public override void Purchase()
    {
    }
}