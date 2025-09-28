using NUnit.Framework.Interfaces;
using UnityEngine;

public class TempWeaponManager : MonoBehaviour
{
    public static TempWeaponManager Instance { get; private set; }
    public Weapon[] playerArmory = new Weapon[6];
    [SerializeField] private WeaponStat[] weaponStats = new WeaponStat[6];
    [SerializeField] private Weapon[] weaponList = new Weapon[6];

    private void Awake()
    {
        Instance = this;
        if (!PlayerPrefs.HasKey("PlayerArmory"))
        {
            InitPlayerArmory();
        }
        playerArmory = GetPlayerArmory();
    }

    private void InitPlayerArmory()
    {
        PlayerPrefs.SetString("PlayerArmory", "Canon|Canon|Canon|Canon|Canon|Canon");
    }

    public int GetWeaponIndex(Weapon weapon)
    {
        for (int i=0;i<6;i++)
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
        string crypted = PlayerPrefs.GetString("PlayerArmory");
        string[] infoArray = crypted.Split("|");
        Weapon[] temp = new Weapon[6];
        for (int i=0;i<6;i++)
        {
            temp[i] = GetWeaponByName(infoArray[i]);
        }
        return temp;
    }

    private Weapon GetWeaponByName(string name)
    {
        foreach(Weapon weapon in weaponList)
        {
            if (weapon.weaponName == name)
            {
                return weapon;
            }
        }
        return null;
    }
}
