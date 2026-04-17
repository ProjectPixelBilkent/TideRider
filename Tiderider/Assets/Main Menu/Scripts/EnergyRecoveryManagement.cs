using UnityEngine;
using System;

public class EnergyRecoveryManager : MonoBehaviour
{
    public static EnergyRecoveryManager Instance { get; private set; }

    private const int MaxEnergy = 5;
    private const int RecoverySeconds = 300; // 5 minutes
    private float timer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadAndCheckOfflineProgress();
    }

    private void Update()
    {
        if (DataManager.GetEnergyAmount() < MaxEnergy)
        {
            timer -= Time.deltaTime;
            if (timer <= 0) RegenerateEnergy();
        }
    }

    private void LoadAndCheckOfflineProgress()
    {
        GameData data = LocalBackupManager.LoadGameData();
        int currentEnergy = data.energyAmount;

        if (currentEnergy >= MaxEnergy)
        {
            timer = RecoverySeconds;
            return;
        }

        if (string.IsNullOrEmpty(data.lastEnergyUpdateTime))
        {
            // First time or no saved time, just start timer
            timer = RecoverySeconds;
            return;
        }

        // Calculate time difference
        DateTime lastUpdate = DateTime.Parse(data.lastEnergyUpdateTime);
        TimeSpan elapsed = DateTime.Now - lastUpdate;

        int totalSecondsElapsed = (int)elapsed.TotalSeconds;
        int energyToAdd = totalSecondsElapsed / RecoverySeconds;
        int remainingSeconds = totalSecondsElapsed % RecoverySeconds;

        if (energyToAdd > 0)
        {
            // Cap energy at MaxEnergy
            int newEnergy = Mathf.Min(currentEnergy + energyToAdd, MaxEnergy);

            // We use a modified save loop here to update both at once
            data.energyAmount = newEnergy;

            if (newEnergy >= MaxEnergy)
            {
                data.lastEnergyUpdateTime = ""; // Reset
                timer = RecoverySeconds;
            }
            else
            {
                // Set timer to the "leftover" time from the last tick
                timer = RecoverySeconds - remainingSeconds;
                data.lastEnergyUpdateTime = DateTime.Now.AddSeconds(-remainingSeconds).ToString();
            }

            LocalBackupManager.SaveGameData(data);
            if (ResourceManager.Instance != null) ResourceManager.Instance.UpdateUI();
        }
        else
        {
            // Less than 5 mins passed, just adjust local timer
            timer = RecoverySeconds - totalSecondsElapsed;
        }
    }

    private void RegenerateEnergy()
    {
        GameData data = LocalBackupManager.LoadGameData();
        data.energyAmount++;

        // If still below max, set the timestamp for the next tick
        data.lastEnergyUpdateTime = (data.energyAmount < MaxEnergy)
            ? DateTime.Now.ToString()
            : "";

        LocalBackupManager.SaveGameData(data);

        if (ResourceManager.Instance != null) ResourceManager.Instance.UpdateUI();
        timer = RecoverySeconds;
    }

    // Call this specifically when a player spends energy to start the clock
    public void OnEnergySpent()
    {
        GameData data = LocalBackupManager.LoadGameData();
        if (string.IsNullOrEmpty(data.lastEnergyUpdateTime))
        {
            data.lastEnergyUpdateTime = DateTime.Now.ToString();
            LocalBackupManager.SaveGameData(data);
        }
        timer = RecoverySeconds;
    }

    public string GetFormattedTime()
    {
        if (DataManager.GetEnergyAmount() >= MaxEnergy) return "FULL";
        TimeSpan t = TimeSpan.FromSeconds(timer);
        return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }
}