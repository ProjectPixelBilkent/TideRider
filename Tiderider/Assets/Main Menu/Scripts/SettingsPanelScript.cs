using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MonoBehaviour class for managing the setting panel in the UI.
/// </summary>
/// <remarks>
/// Created by: Işık Dönger
/// </remarks>
public class SettingsPanelScript : MonoBehaviour
{
    [SerializeField] private GameObject SfxSlider;
    [SerializeField] private GameObject MusicSlider;
    [SerializeField] private GameObject LanguageMenu;
    [SerializeField] private GameObject CreditsBtn;
    [SerializeField] private GameObject FeedbackBtn;
    private Slider sfxSliderComponent, musicSliderComponent;
    public static float sfxVolume;
    public static float musicVolume;

    private void Start()
    {
        LanguageMenu.SetActive(false);
        sfxSliderComponent = SfxSlider.GetComponent<Slider>();
        musicSliderComponent = MusicSlider.GetComponent<Slider>();
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSliderComponent.value = sfxVolume;
        musicSliderComponent.value = musicVolume;
    }

    /// <summary>
    /// Changes the SFX volume based on the slider value and saves it to PlayerPrefs.
    /// <summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void ChangeSFXVolume()
    {
        sfxVolume = sfxSliderComponent.value;
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    /// <summary>
    /// Changes the Music volume based on the slider value and saves it to PlayerPrefs.
    /// <summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void ChangeMusicVolume()
    {
        musicVolume = musicSliderComponent.value;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    /// <summary>
    /// Opens or closes Language Menu based on active state.
    /// <summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void Open_CloseLanguageMenu()
    {
        if (LanguageMenu.activeSelf == false)
        {
            CreditsBtn.SetActive(false);
            FeedbackBtn.SetActive(false);
            LanguageMenu.SetActive(true);
        }
        else
        {
            LanguageMenu.SetActive(false);
            FeedbackBtn.SetActive(true);
            CreditsBtn.SetActive(true);
        }
    }

    /*public async void OpenMailApp()
    {
        string mail = await FirestoreManager.GetSupportMail();
        string mailtoUrl = $"mailto:{mail}";
        Application.OpenURL(mailtoUrl);
    }*/
}