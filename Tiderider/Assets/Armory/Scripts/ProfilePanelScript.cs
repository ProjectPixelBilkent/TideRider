using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// MonoBehaviour class for managing the profile panel in the UI.
/// </summary>
/// <todo>
/// Implement linking to account through various sign in methods.
/// </todo>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
public class ProfilePanelScript : MonoBehaviour
{
    [SerializeField] private List<GameObject> weaponProgressList;
    [SerializeField] private List<Weapon> weaponList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitiliazeWeaponLevelInfo();
    }

    /// <summary>
    /// Initializes the weapon level information in the profile panel.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    private void InitiliazeWeaponLevelInfo()
    {
        List<int> weaponLevels = DataManager.GetWeaponLevels();

        for (int i = 0; i < Mathf.Min(weaponLevels.Count, weaponProgressList.Count, weaponList.Count); i++)
        {
            StringBuilder progressString = new StringBuilder(weaponLevels[i]);
            progressString.Append("/");
            progressString.Append(weaponList[i].weaponLevelCount);
            weaponProgressList[i].GetComponent<TMP_Text>().text = progressString.ToString();
        }
    }

    /// <summary>
    /// Links the player's account to Play Games services.
    /// </summary>
    /// <todo>
    /// Implement the Link to Play Games account and handle the sign-in/sing-out process.
    /// </todo>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void LinkToPlayGames()
    {

    }

    /// <summary>
    /// Links the player's account to Apple services.
    /// </summary>
    /// <todo>
    /// Implement the Link to Apple Id account and handle the sign-in/sing-out process.
    /// </todo>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void LinkToAppleId()
    {

    }

    /// <summary>
    /// Links the player's account to Facebook services.
    /// </summary>
    /// <todo>
    /// Implement the Link to Facebook account and handle the sign-in/sing-out process.
    /// </todo>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void LinkToFacebook()
    {

    }
}
