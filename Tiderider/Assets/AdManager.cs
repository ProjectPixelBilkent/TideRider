using UnityEngine;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Initialize LevelPlay here tonight
        // IronSource.Agent.init("YOUR_APP_KEY");
    }

    public void ShowRewardedAd(Action onComplete)
    {
        // If they bought No Ads, just give the reward immediately!
        if (DataManager.HasRemovedAds())
        {
            onComplete?.Invoke();
            return;
        }

        // Logic for LevelPlay/Ads
        // if(IronSource.Agent.isRewardedVideoAvailable()) {
        //    IronSource.Agent.showRewardedVideo();
        //    ... handle success callback ...
        // }

        // FOR TESTING TONIGHT: Just invoke the reward
        Debug.Log("Showing Ad...");
        onComplete?.Invoke();
    }
}