using NUnit.Framework.Interfaces;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance { get; private set; }
    public Weapon[] playerArmory = new Weapon[6];
    [SerializeField] private WeaponStat[] weaponStats = new WeaponStat[6];

    private void Awake()
    {
        Instance = this;
        playerArmory = DataManager.GetPlayerArmory();
    }

    public int GetWeaponIndex(Weapon weapon)
    {
        for (int i = 0; i < playerArmory.Length; i++)
        {
            if (playerArmory[i] == weapon)
            {
                return i;
            }
        }
        return -1;
    }

    public int GetWeaponLevel(Weapon weapon)
    {
        foreach (var weaponStat in weaponStats)
        {
            if (weaponStat.weaponInfo == weapon)
            {
                return weaponStat.level;
            }
        }
        return -1;
    }

    public Weapon[] GetPlayerArmory()
    {
        Debug.Log("Getting player armory");
        foreach (var weapon in playerArmory)
        {
            Debug.Log(weapon ? weapon.weaponName : "Empty slot");
        }
        return playerArmory;
    }
}