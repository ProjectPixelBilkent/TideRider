using UnityEngine;
using System;

public class EnergyRecoveryManager : MonoBehaviour
{
    public static EnergyRecoveryManager Instance { get; private set; }

    private const int MaxEnergy = 5;
    private const int RecoverySeconds = 5; // 5 seconds for testing purposes, change to 300 for 5 minutes in production
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
            timer = RecoverySeconds;
            return;
        }

        TimeSpan elapsed = TimeManager.GetTimePassed(data.lastEnergyUpdateTime);

        int totalSecondsElapsed = (int)elapsed.TotalSeconds;
        int energyToAdd = totalSecondsElapsed / RecoverySeconds;
        int remainingSeconds = totalSecondsElapsed % RecoverySeconds;

        if (energyToAdd > 0)
        {
            int newEnergy = Mathf.Min(currentEnergy + energyToAdd, MaxEnergy);
            data.energyAmount = newEnergy;

            if (newEnergy >= MaxEnergy)
            {
                data.lastEnergyUpdateTime = "";
                timer = RecoverySeconds;
            }
            else
            {
                timer = RecoverySeconds - remainingSeconds;
                data.lastEnergyUpdateTime = TimeManager.GetAdjustedTimeString(-remainingSeconds);
            }

            LocalBackupManager.SaveGameData(data);
            if (ResourceManager.Instance != null) ResourceManager.Instance.UpdateUI();
        }
        else
        {
            timer = RecoverySeconds - totalSecondsElapsed;
        }
    }

    private void RegenerateEnergy()
    {
        GameData data = LocalBackupManager.LoadGameData();
        data.energyAmount++;

        data.lastEnergyUpdateTime = (data.energyAmount < MaxEnergy)
            ? TimeManager.GetCurrentTimeString()
            : "";

        LocalBackupManager.SaveGameData(data);

        if (ResourceManager.Instance != null) ResourceManager.Instance.UpdateUI();
        timer = RecoverySeconds;
    }

    public void OnEnergySpent()
    {
        GameData data = LocalBackupManager.LoadGameData();
        if (string.IsNullOrEmpty(data.lastEnergyUpdateTime))
        {
            data.lastEnergyUpdateTime = TimeManager.GetCurrentTimeString();
            LocalBackupManager.SaveGameData(data);
        }
        timer = RecoverySeconds;
    }

    public string GetFormattedTime()
    {
        if (DataManager.GetEnergyAmount() >= MaxEnergy) return string.Empty;
        TimeSpan t = TimeSpan.FromSeconds(timer);
        return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }
}
