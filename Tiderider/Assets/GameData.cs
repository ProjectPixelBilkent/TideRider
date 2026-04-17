using UnityEngine;
using System.Collections.Generic;
using System;
using JetBrains.Annotations;

/// <summary>
/// Serializable class for managing game data.
/// </summary>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
[Serializable]
public class GameData
{
    public int coinAmount = 0;
    public int energyAmount = 5;
    public Weapon[] playerArmory = new Weapon[6];
    public int highestUnlockedLevelIndex = 0;
    public List<string> completedConversations = new List<string>();
    public string lastEnergyUpdateTime = "";
}

/// <summary>
/// Serializable class for managing weapon data.
/// </summary>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
[Serializable]
public class WeaponData
{
    public int canonLevel = 0;
    public int minigunLevel = 0;
    public int shieldLevel = 0;
    public int lasergunLevel = 0;
    public int flamethrowerLevel = 0;
    public int icegunLevel = 0;
}
