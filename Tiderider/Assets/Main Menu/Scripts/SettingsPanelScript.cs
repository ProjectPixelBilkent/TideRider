using UnityEngine;
using UnityEngine.UIElements;

public class SettingsPanelScript : MonoBehaviour
{
    [SerializeField] private GameObject SfxSlider;
    [SerializeField] private GameObject MusicSlider;
    [SerializeField] private GameObject LanguageMenu;
    [SerializeField] private GameObject CreditsBtn;
    [SerializeField] private GameObject FeedbackBtn;
    public static float sfxVolume;
    public static float musicVolume;

    private void Start()
    {
        LanguageMenu.SetActive(false);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        //SfxSlider.GetComponent<Slider>().value = sfxVolume;
        //MusicSlider.GetComponent<Slider>().value = musicVolume;
    }

    public void ChangeSFXVolume()
    {
        sfxVolume = SfxSlider.GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }
    public void ChangeMusicVolume()
    {
        musicVolume = MusicSlider.GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }
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