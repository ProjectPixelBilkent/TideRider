using UnityEngine;
using static LocalBackupManager;

public static class DataManager
{
    /// <summary>
    /// Loads the current canon level from the local backup.
    /// </summary>
    /// <returns>Current Canon Level.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetCanonLevel() => LoadWeaponData().canonLevel;

    /// <summary>
    /// Loads the current minigun level from the local backup.
    /// </summary>
    /// <returns>Current Minigun Level.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetMinigunLevel() => LoadWeaponData().minigunLevel;

    /// <summary>
    /// Loads the current shield level from the local backup.
    /// </summary>
    /// <returns>Current Shield Level.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetShieldLevel() => LoadWeaponData().shieldLevel;

    /// <summary>
    /// Loads the current flamethrower level from the local backup.
    /// </summary>
    /// <returns>Current Flamethrower Level.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetFlamethrowerLevel() => LoadWeaponData().flamethrowerLevel;

    /// <summary>
    /// Loads the current icegun level from the local backup.
    /// </summary>
    /// <returns>Current Icegun Level.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetIcegunLevel() => LoadWeaponData().icegunLevel;

    /// <summary>
    /// Loads the current laser level from the local backup.
    /// </summary>
    /// <returns>Current Laser Level.</returns>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public static int GetLaserLevel() => LoadWeaponData().laserLevel;
}
