using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private TMP_Text energyAmount;
    [SerializeField] private TMP_Text energyTimerText;
    [SerializeField] private TMP_Text coinAmount;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        energyAmount.text = DataManager.GetEnergyAmount() + "/5";
        coinAmount.text = DataManager.GetCoinAmount().ToString();
    }

    void Update()
    {
        if (energyTimerText != null)
        {
            energyTimerText.text = EnergyRecoveryManager.Instance.GetFormattedTime();
        }
    }

    public static bool isEnergyLeft()
    {
        return DataManager.GetEnergyAmount() > 0;
    }

    public static void HandleNoEnergy()
    {
        NotificationManager.Instance.ShowNotification("Not enough energy!");
    }

    public static void UpgradeWeapon(Weapon weapon)
    {
        int currentLevel = DataManager.GetWeaponLevels()[WeaponManager.Instance.GetWeaponIndex(weapon)];

        if (currentLevel >= weapon.weaponLevels.Length) return;

        int cost = weapon.weaponLevels[currentLevel].cost;

        if (DataManager.GetCoinAmount() < cost)
        {
            HandleNoCoin();
        }
        else
        {
            DataManager.SubtractCoinAmount(cost);
            Instance.UpdateUI();
        }
    }

    private static void HandleNoCoin()
    {
        NotificationManager.Instance.ShowNotification("Not enough coins!", NotificationManager.UITab.Shop);
    }
}