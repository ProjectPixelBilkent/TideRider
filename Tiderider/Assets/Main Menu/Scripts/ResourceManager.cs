using TMPro;
using UnityEngine;
using DG.Tweening;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private TMP_Text energyAmount;
    [SerializeField] private TMP_Text coinAmount;

    [Header("Notification System")]
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform notificationPanel;

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
        Debug.Log(DataManager.GetCoinAmount());
    }

    public static bool isEnergyLeft()
    {
        return DataManager.GetEnergyAmount() > 0;
    }

    public static void HandleNoEnergy()
    {
        Instance.ShowNotification("Not enough energy!");
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
        Instance.ShowNotification("Not enough coins!");
    }

    public void ShowNotification(string message)
    {
        GameObject go = Instantiate(notificationPrefab, notificationPanel);
        go.GetComponent<NotificationItem>().Setup(message);

        go.transform.SetAsLastSibling();
    }
}