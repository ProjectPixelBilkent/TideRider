using UnityEngine;
using Unity.Services.LevelPlay;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    [Header("LevelPlay Settings")]
    [SerializeField] private string androidAppKey = "261284bd5";
    [SerializeField] private string androidAdUnitId = "pivv358jdy1664z4";

    [SerializeField] private string iosAppKey = "26127d8c5";
    [SerializeField] private string iosAdUnitId = "d1b1phyldw8ffqnl";

    private string currentAppKey;
    private string currentAdUnitId;

    private LevelPlayRewardedAd rewardedAd;
    private Action onRewardSuccess;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

#if UNITY_ANDROID
        currentAppKey = androidAppKey;
        currentAdUnitId = androidAdUnitId;
#elif UNITY_IOS
            currentAppKey = iosAppKey;
            currentAdUnitId = iosAdUnitId;
#else
            currentAppKey = "UNUSED";
            currentAdUnitId = "UNUSED";
#endif
    }

    void Start()
    {
        // Setup initialization callbacks
        LevelPlay.OnInitSuccess += OnLevelPlayInitSuccess;
        LevelPlay.OnInitFailed += OnLevelPlayInitFailed;

        // Start the SDK
        LevelPlay.Init(currentAppKey);
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
        rewardedAd = new LevelPlayRewardedAd(currentAdUnitId);

        rewardedAd.OnAdLoaded += (adInfo) => { Debug.Log("REWARDED AD LOADED AND READY"); };

        // Subscribe to reward event
        rewardedAd.OnAdRewarded += (reward, adInfo) => {
            Debug.Log("Ad Rewarded - Executing Callback");
            onRewardSuccess?.Invoke();
            onRewardSuccess = null;

            // Immediately start loading the next one
            rewardedAd.LoadAd();
        };

        // Subscribe to failed display to clean up the action if the ad crashes
        rewardedAd.OnAdDisplayFailed += (error, adInfo) => {
            onRewardSuccess = null;
        };

        // Load the initial ad
        rewardedAd.LoadAd();
    }

    public void ShowRewardedAd(Action onSuccess)
    {
        // Bypass if user has "No-Ads" purchased
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
            Debug.LogWarning("Ad not ready yet. Reloading...");
            rewardedAd?.LoadAd();
        }
    }
}