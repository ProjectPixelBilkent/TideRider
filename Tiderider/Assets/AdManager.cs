using UnityEngine;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    public void ShowRewardedAd(Action onSuccess)
    {
        // Bypass if user has "No-Ads" purchased
        if (DataManager.HasRemovedAds())
        {
            onSuccess?.Invoke();
            return;
        }

        NotificationManager.Instance.ShowNotification("Ads are currently unavailable.");
    }
}