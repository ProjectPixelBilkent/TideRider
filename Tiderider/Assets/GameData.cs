using UnityEngine;
using System.Collections.Generic;
using System;
using JetBrains.Annotations;

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
    public int flamethrowerLevel;
    public int icegunLevel;
    public int laserLevel;
}
