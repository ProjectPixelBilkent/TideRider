using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [Header("Item Data (ONLY for Energy & Coin)")]
    public ShopItemData itemData;

    [HideInInspector] public Button entireFrameButton;
    [HideInInspector] public TMP_Text nameText;
    [HideInInspector] public Image iconImage;
    [HideInInspector] public TMP_Text levelOrDescText;
    [HideInInspector] public TMP_Text costText;

    private void Awake()
    {
        entireFrameButton = GetComponent<Button>();
        entireFrameButton.onClick.AddListener(OnItemClicked);

        if (transform.childCount >= 4)
        {
            nameText = transform.GetChild(0).GetComponent<TMP_Text>();
            iconImage = transform.GetChild(1).GetComponent<Image>();
            levelOrDescText = transform.GetChild(2).GetComponent<TMP_Text>();
            costText = transform.GetChild(3).GetComponent<TMP_Text>();
        }
        else
        {
            Debug.LogWarning("ShopItem prefab hierarchy child count is less than 4. UI references will fail.", this);
        }
    }

    private void Start()
    {
        if (itemData != null)
        {
            SetupStaticUI();
        }
    }

    private void Update()
    {
        if (itemData is EnergyItemData energy && energy.isAdReward)
        {
            UpdateEnergyAdUI(energy);
        }
    }

    private void SetupStaticUI()
    {
        if (nameText != null) nameText.text = itemData.itemName;
        if (iconImage != null && itemData.itemIcon != null)
            iconImage.sprite = itemData.itemIcon;

        switch (itemData)
        {
            case EnergyItemData energy:
                levelOrDescText.text = $"+{energy.amountToGive} Energy";
                costText.text = energy.isAdReward ? "FREE (AD)" : $"{energy.coinCost} COINS";
                break;

            case CoinItemData coin:
                levelOrDescText.text = $"{coin.amountToGive} Coins";
                costText.text = "BUY";
                break;
        }
    }

    private void UpdateEnergyAdUI(EnergyItemData energy)
    {
        bool ready = TimeManager.HasPassed(
            DataManager.GetLastEnergyAdTime(),
            TimeSpan.FromHours(1)
        );

        if (costText != null)
        {
            costText.text = ready
            ? "FREE (AD)"
            : TimeManager.GetRemainingTimeFormatted(
                DataManager.GetLastEnergyAdTime(),
                TimeSpan.FromHours(1)
              );
        }

        if (entireFrameButton != null)
        {
            entireFrameButton.interactable = ready;
        }
    }

    private void OnItemClicked()
    {
        if (itemData != null)
        {
            ShopManager.Instance.ProcessPurchase(this);
        }
    }
}