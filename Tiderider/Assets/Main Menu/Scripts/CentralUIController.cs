using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class CentralUIController : MonoBehaviour
{
    public static CentralUIController Instance { get; private set; }

    [Header("UI References")]
    public RectTransform MainMenuCanvas, ArmoryCanvas, ShopCanvas;
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
        ArmoryCanvas.DOAnchorPosX(0f, 1f);
        MainMenuCanvas.DOAnchorPosX(1080f, 1f);
        ShopCanvas.DOAnchorPosX(2160f, 1f);
    }

    public void OpenMainMenu()
    {
        ArmoryCanvas.DOAnchorPosX(-1080f, 1f);
        MainMenuCanvas.DOAnchorPosX(0f, 1f);
        ShopCanvas.DOAnchorPosX(1080f, 1f);
    }

    public void OpenShop()
    {
        ArmoryCanvas.DOAnchorPosX(-2160f, 1f);
        MainMenuCanvas.DOAnchorPosX(-1080f, 1f);
        ShopCanvas.DOAnchorPosX(0f, 1f);
    }
}