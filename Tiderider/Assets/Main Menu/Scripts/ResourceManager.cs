using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private TMP_Text energyAmount;
    [SerializeField] private TMP_Text energyTimerText;
    [SerializeField] private TMP_Text coinAmount;
    private bool hasLoggedMissingReferences;

    void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (energyAmount == null || coinAmount == null)
        {
            if (!hasLoggedMissingReferences)
            {
                Debug.LogWarning("ResourceManager.UpdateUI skipped because UI text references are missing.", this);
                hasLoggedMissingReferences = true;
            }
            return;
        }

        hasLoggedMissingReferences = false;
        energyAmount.text = DataManager.GetEnergyAmount() + "/5";
        coinAmount.text = DataManager.GetCoinAmount().ToString();
    }

    void Update()
    {
        if (energyTimerText != null && EnergyRecoveryManager.Instance != null)
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
