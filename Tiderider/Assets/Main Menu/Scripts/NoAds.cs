using UnityEngine;
using UnityEngine.Purchasing;

public class NoAds : MonoBehaviour
{
    private void Awake()
    {
        // Check if already bought on startup to hide the button
        //CheckAndHideIfPurchased();
    }

    /// <summary>
    /// Hides the button if the user already owns the No Ads upgrade.
    /// </summary>
    private void CheckAndHideIfPurchased()
    {
        if (DataManager.HasRemovedAds())
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called by the IAP Button's 'On Purchase Confirmed' or 'On Purchase Succeeded' event.
    /// </summary>
    public void OnNoAdsPurchaseComplete(Product product)
    {
        Debug.Log($"Purchase successful: {product.definition.id}");

        // 1. Update the persistent data
        DataManager.UnlockNoAds();

        // 2. Refresh the UI or hide the button
        CheckAndHideIfPurchased();

        // 3. (Optional) Show notification
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification("Ads Removed Successfully!");
        }
    }

    public void OnPurchaseFailed(Product product)
    {
        NotificationManager.Instance.ShowNotification("Purchase Failed");
    }
}