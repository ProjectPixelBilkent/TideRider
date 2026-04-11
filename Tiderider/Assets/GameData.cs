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
    public int coinAmount;
    public Weapon[] playerArmory = new Weapon[6];
    public int highestUnlockedLevelIndex = 0;
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
    public int canonLevel;
    public int minigunLevel;
    public int shieldLevel;
    public int lasergunLevel;
    public int flamethrowerLevel;
    public int icegunLevel;
}
