using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItem : MonoBehaviour
{
    [Header("Item Data")]
    public ShopItemData itemData;

    [Tooltip("Only Weapon: 0, 1, 2, 3")]
    public int frameIndex;

    [HideInInspector] public Button entireFrameButton;
    [HideInInspector] public TMP_Text nameText;
    [HideInInspector] public Image iconImage;
    [HideInInspector] public TMP_Text costText;
    [HideInInspector] public TMP_Text levelOrDescText;

    private void Awake()
    {
        entireFrameButton = GetComponent<Button>();

        entireFrameButton.onClick.AddListener(OnItemClicked);

        nameText = transform.Find("WeaponName")?.GetComponent<TMP_Text>();
        iconImage = transform.Find("WeaponIcon")?.GetComponent<Image>();
        levelOrDescText = transform.Find("WeaponDescription")?.GetComponent<TMP_Text>();
        costText = transform.Find("Upgrade/Text (TMP)")?.GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if (itemData != null && itemData.type != ShopItemData.ItemType.Weapon)
        {
            SetupStaticUI();
        }
    }

    private void SetupStaticUI()
    {
        if (nameText != null) nameText.text = itemData.itemName;
        if (iconImage != null && itemData.itemIcon != null) iconImage.sprite = itemData.itemIcon;

        if (itemData.type == ShopItemData.ItemType.Energy)
        {
            if (itemData.isAdReward)
            {
                if (costText != null) costText.text = "FREE (AD)";
                if (levelOrDescText != null) levelOrDescText.text = $"+{itemData.amountToGive} Energy";
            }
            else
            {
                if (costText != null) costText.text = $"{itemData.coinCost} COINS";
                if (levelOrDescText != null) levelOrDescText.text = $"+{itemData.amountToGive} Energy";
            }
        }
        else if (itemData.type == ShopItemData.ItemType.Coin)
        {
            if (costText != null) costText.text = "BUY";
            if (levelOrDescText != null) levelOrDescText.text = $"{itemData.amountToGive} Coins";
        }
    }

    private void OnItemClicked()
    {
        ShopManager.Instance.ProcessPurchase(this);
    }
}