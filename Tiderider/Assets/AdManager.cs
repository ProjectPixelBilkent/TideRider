using UnityEngine;
using Unity.Services.LevelPlay;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    [Header("LevelPlay Settings")]
    [SerializeField] private string appKey = "YOUR_APP_KEY";
    [SerializeField] private string adUnitId = "YOUR_REWARDED_UNIT_ID"; // From Dashboard

    private LevelPlayRewardedAd rewardedAd;
    private Action onRewardSuccess;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    void Start()
    {
        // 1. Initialize LevelPlay
        LevelPlay.OnInitSuccess += OnLevelPlayInitSuccess;
        LevelPlay.OnInitFailed += OnLevelPlayInitFailed;
        LevelPlay.Init(appKey);
    }

    private void OnLevelPlayInitSuccess(LevelPlayConfiguration config)
    {
        Debug.Log("LevelPlay Initialized!");
        CreateRewardedAd();
    }

    private void OnLevelPlayInitFailed(LevelPlayInitError error)
    {
        Debug.LogError($"LevelPlay Init Failed: {error.ErrorMessage}");
    }

    private void CreateRewardedAd()
    {
        // Create the ad object
        rewardedAd = new LevelPlayRewardedAd(adUnitId);

        // Subscribe to events
        rewardedAd.OnAdLoaded += (adInfo) => Debug.Log("Ad Loaded");
        rewardedAd.OnAdDisplayed += (adInfo) => Debug.Log("Ad Displayed");
        rewardedAd.OnAdDisplayFailed += (error, adInfo) => Debug.LogError("Ad Failed");

        // THE REWARD CALLBACK
        rewardedAd.OnAdRewarded += (reward, adInfo) => {
            onRewardSuccess?.Invoke();
            onRewardSuccess = null;
            rewardedAd.LoadAd(); // Preload next ad
        };

        // Load the first ad
        rewardedAd.LoadAd();
    }

    public void ShowRewardedAd(Action onSuccess)
    {
        // Bypass if user has No-Ads
        if (DataManager.HasRemovedAds())
        {
            onSuccess?.Invoke();
            return;
        }

        if (rewardedAd != null && rewardedAd.IsAdReady())
        {
            onRewardSuccess = onSuccess;
            rewardedAd.ShowAd();
        }
        else
        {
            Debug.LogWarning("Ad not ready. Trying to load...");
            rewardedAd?.LoadAd();
        }
    }
}