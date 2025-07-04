using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CentralUIController : MonoBehaviour
{
    public static CentralUIController Instance { get; private set; }

    [Header("UI References")]
    public CanvasGroup LevelMenu;

    private CanvasGroup _currentMenu;
    private Coroutine _activeTransition;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ToggleMenu(CanvasGroup targetMenu)
    {
        if (_activeTransition != null)
            StopCoroutine(_activeTransition);

        _activeTransition = StartCoroutine(ToggleMenuRoutine(targetMenu));
    }

    private IEnumerator ToggleMenuRoutine(CanvasGroup target)
    {
        // Close current menu if different
        if (_currentMenu != null && _currentMenu != target)
        {
            yield return _currentMenu.FadeOut(this);
            _currentMenu = null;
        }

        // Toggle target menu
        if (_currentMenu == target)
        {
            yield return target.FadeOut(this);
            _currentMenu = null;
            yield return LevelMenu.FadeIn(this);
        }
        else
        {
            yield return LevelMenu.FadeOut(this);
            yield return target.FadeIn(this);
            _currentMenu = target;
        }

        _activeTransition = null;
    }

    public void OpenArmory()
    {
        SceneManager.LoadScene("Armory", LoadSceneMode.Single);
    }

    public void OpenMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void OpenShop()
    {
        SceneManager.LoadScene("Shop", LoadSceneMode.Single);
    }
}