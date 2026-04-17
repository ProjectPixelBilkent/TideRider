using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }

    [Header("Notification System")]
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform notificationPanel;

    public enum UITab
    {
        Armory = 0,
        MainMenu = 1,
        Shop = 2
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Spawns a standard notification message.
    /// </summary>
    public void ShowNotification(string message)
    {
        GameObject go = Instantiate(notificationPrefab, notificationPanel);
        go.GetComponent<NotificationItem>().Setup(message);

        go.transform.SetAsLastSibling();
    }

    /// <summary>
    /// Spawns a notification message and immediately switches to the requested UI tab.
    /// </summary>
    public void ShowNotification(string message, UITab tabToOpen)
    {
        // First, show the notification
        ShowNotification(message);

        // Then, route to the correct tab via CentralUIController
        if (CentralUIController.Instance != null)
        {
            switch (tabToOpen)
            {
                case UITab.Armory:
                    CentralUIController.Instance.OpenArmory();
                    break;
                case UITab.MainMenu:
                    CentralUIController.Instance.OpenMainMenu();
                    break;
                case UITab.Shop:
                    CentralUIController.Instance.OpenShop();
                    break;
            }
        }
        else
        {
            Debug.LogWarning("CentralUIController Instance is missing! Cannot change tabs.");
        }
    }
}