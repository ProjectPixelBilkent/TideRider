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
    public GameObject IconPanel;
    public RectTransform ArmoryCanvas, LevelCanvas, ShopCanvas;

    private CanvasGroup activeCanvas;
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

        activeCanvas = LevelCanvas.GetComponentInChildren<CanvasGroup>();
    }

    /// <summary>
    /// Toggles the visibility of a menu using a fade transition.
    /// </summary>
    /// <param name="targetMenu">The CanvasGroup of the menu to toggle</param>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void ToggleMenu(CanvasGroup targetMenu)
    {
        if (_activeTransition != null)
            StopCoroutine(_activeTransition);

        _activeTransition = StartCoroutine(ToggleMenuRoutine(targetMenu));
    }

    /// <summary>
    /// Coroutine that handles the fading in/out transitions for menu toggling.
    /// </summary>
    /// <param name="target">The target CanvasGroup to toggle</param>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
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
            yield return activeCanvas.FadeIn(this);
        }
        else
        {
            yield return activeCanvas.FadeOut(this);
            yield return target.FadeIn(this);
            _currentMenu = target;
        }

        _activeTransition = null;
    }

    /// <summary>
    /// Opens the armory screen and updates the icon panel visuals.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void OpenArmory()
    {
        RectTransform SelectedIconRect = IconPanel.transform.GetChild(0).GetComponent<RectTransform>(),
            SideIconRect1 = IconPanel.transform.GetChild(1).GetComponent<RectTransform>(),
            SideIconRect2 = IconPanel.transform.GetChild(2).GetComponent<RectTransform>();

        activeCanvas = ArmoryCanvas.GetComponentInChildren<CanvasGroup>();

        ArmoryCanvas.DOAnchorPosX(0f, 1f);
        LevelCanvas.DOAnchorPosX(ScaleManager.Width, 1f);
        ShopCanvas.DOAnchorPosX(ScaleManager.Width * 2, 1f);

        SelectedIconRect.DOSizeDelta(new Vector2(ScaleManager.SelectedIconWidth, SelectedIconRect.sizeDelta.y), 0.5f);
        SelectedIconRect.DOAnchorPosX(ScaleManager.SelectedIconWidth / 2, 0.5f);
        SelectedIconRect.GetComponent<Image>().color = ScaleManager.SelectedColor;
        SideIconRect1.DOSizeDelta(new Vector2(ScaleManager.SideIconWidth, SideIconRect1.sizeDelta.y), 0.5f);
        SideIconRect1.DOAnchorPosX((ScaleManager.SelectedIconWidth - ScaleManager.SideIconWidth) / 2, 0.5f);
        SideIconRect1.GetComponent<Image>().color = ScaleManager.SideColor;
        SideIconRect2.DOSizeDelta(new Vector2(ScaleManager.SideIconWidth, SideIconRect2.sizeDelta.y), 0.5f);
        SideIconRect2.DOAnchorPosX(-ScaleManager.SideIconWidth / 2, 0.5f);
        SideIconRect2.GetComponent<Image>().color = ScaleManager.SideColor;
    }

    /// <summary>
    /// Opens the main menu screen and updates the icon panel visuals.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void OpenMainMenu()
    {
        RectTransform SelectedIconRect = IconPanel.transform.GetChild(1).GetComponent<RectTransform>(),
            SideIconRect1 = IconPanel.transform.GetChild(0).GetComponent<RectTransform>(),
            SideIconRect2 = IconPanel.transform.GetChild(2).GetComponent<RectTransform>();

        activeCanvas = LevelCanvas.GetComponentInChildren<CanvasGroup>();

        ArmoryCanvas.DOAnchorPosX(-ScaleManager.Width, 1f);
        LevelCanvas.DOAnchorPosX(0f, 1f);
        ShopCanvas.DOAnchorPosX(ScaleManager.Width, 1f);

        SideIconRect1.DOSizeDelta(new Vector2(ScaleManager.SideIconWidth, SideIconRect1.sizeDelta.y), 0.5f);
        SideIconRect1.DOAnchorPosX(ScaleManager.SideIconWidth / 2, 0.5f);
        SideIconRect1.GetComponent<Image>().DOColor(ScaleManager.SideColor, 0.5f);
        SelectedIconRect.DOSizeDelta(new Vector2(ScaleManager.SelectedIconWidth, SelectedIconRect.sizeDelta.y), 0.5f);
        SelectedIconRect.DOAnchorPosX(0f, 0.5f);
        SelectedIconRect.GetComponent<Image>().DOColor(ScaleManager.SelectedColor, 0.5f);
        SideIconRect2.DOSizeDelta(new Vector2(ScaleManager.SideIconWidth, SideIconRect2.sizeDelta.y), 0.5f);
        SideIconRect2.DOAnchorPosX(-ScaleManager.SideIconWidth / 2, 0.5f);
        SideIconRect2.GetComponent<Image>().DOColor(ScaleManager.SideColor, 0.5f);
    }

    /// <summary>
    /// Opens the shop screen and updates the icon panel visuals.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void OpenShop()
    {
        RectTransform SelectedIconRect = IconPanel.transform.GetChild(2).GetComponent<RectTransform>(),
            SideIconRect1 = IconPanel.transform.GetChild(0).GetComponent<RectTransform>(),
            SideIconRect2 = IconPanel.transform.GetChild(1).GetComponent<RectTransform>();

        activeCanvas = ShopCanvas.GetComponentInChildren<CanvasGroup>();

        ArmoryCanvas.DOAnchorPosX(-ScaleManager.Width * 2, 1f);
        LevelCanvas.DOAnchorPosX(-ScaleManager.Width, 1f);
        ShopCanvas.DOAnchorPosX(0f, 1f);

        SideIconRect1.DOSizeDelta(new Vector2(ScaleManager.SideIconWidth, SideIconRect1.sizeDelta.y), 0.5f);
        SideIconRect1.DOAnchorPosX(ScaleManager.SideIconWidth / 2, 0.5f);
        SideIconRect1.GetComponent<Image>().DOColor(ScaleManager.SideColor, 0.5f);
        SideIconRect2.DOSizeDelta(new Vector2(ScaleManager.SideIconWidth, SideIconRect2.sizeDelta.y), 0.5f);
        SideIconRect2.DOAnchorPosX(-(ScaleManager.SelectedIconWidth - ScaleManager.SideIconWidth) / 2, 0.5f);
        SideIconRect2.GetComponent<Image>().DOColor(ScaleManager.SideColor, 0.5f);
        SelectedIconRect.DOSizeDelta(new Vector2(ScaleManager.SelectedIconWidth, SelectedIconRect.sizeDelta.y), 0.5f);
        SelectedIconRect.DOAnchorPosX(-ScaleManager.SelectedIconWidth / 2, 0.5f);
        SelectedIconRect.GetComponent<Image>().DOColor(ScaleManager.SelectedColor, 0.5f);
    }
}