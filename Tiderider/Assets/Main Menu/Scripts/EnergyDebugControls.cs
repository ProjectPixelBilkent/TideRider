using System;
using UnityEngine;

public class EnergyDebugControls : MonoBehaviour
{
    [SerializeField] [Min(0)] private int energyToSet = 5;
    [SerializeField] [Min(0)] private int maxEnergy = 5;

    public void FillEnergyToMax()
    {
        SetEnergy(maxEnergy);
    }

    public void ApplyCustomEnergy()
    {
        SetEnergy(energyToSet);
    }

    private void SetEnergy(int amount)
    {
        GameData data = LocalBackupManager.LoadGameData();
        data.energyAmount = Mathf.Clamp(amount, 0, maxEnergy);
        data.lastEnergyUpdateTime = data.energyAmount >= maxEnergy ? string.Empty : DateTime.Now.ToString();

        LocalBackupManager.SaveGameData(data);

        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.UpdateUI();
        }
    }
}
