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
    public RectTransform ArmoryCanvas, LevelCanvas, ShopCanvas, scrollContentTransform;

    private CanvasGroup activeCanvas;
    private CanvasGroup _currentPanel;
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

    private void Update()
    {
        
    }

    /// <summary>
    /// Toggles the visibility of a panel using a fade transition.
    /// </summary>
    /// <param name="targetPanel">The CanvasGroup of the panel to toggle</param>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void TogglePanel(CanvasGroup targetPanel)
    {
        if (_activeTransition != null)
            StopCoroutine(_activeTransition);

        _activeTransition = StartCoroutine(TogglePanelRoutine(targetPanel));
    }

    /// <summary>
    /// Coroutine that handles the fading in/out transitions for panel toggling.
    /// </summary>
    /// <param name="target">The target CanvasGroup to toggle</param>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    private IEnumerator TogglePanelRoutine(CanvasGroup target)
    {
        // Close current panel if different
        if (_currentPanel != null && _currentPanel != target)
        {
            yield return _currentPanel.FadeOut(this);
            _currentPanel = null;
        }

        // Toggle target panel
        if (_currentPanel == target)
        {
            yield return target.FadeOut(this);
            _currentPanel = null;
            yield return activeCanvas.FadeIn(this);
        }
        else
        {
            yield return activeCanvas.FadeOut(this);
            yield return target.FadeIn(this);
            _currentPanel = target;
        }

        _activeTransition = null;
    }

    /// <summary>
    /// Manages menu changes for scrolling and updates the icon panel accordingly.
    /// </summary>
    /// <remarks>
    /// Maintained by: Işık Dönger
    /// </remarks>
    public void ChangeMenu()
    {
        if (scrollContentTransform.anchoredPosition.x > -1080)
        {
            OpenArmory();
        }
        else if (scrollContentTransform.anchoredPosition.x > -2160)
        {
            OpenMainMenu();
        }
        else
        {
            OpenShop();
        }
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

        if (_currentPanel != null) { _currentPanel.FadeOut(this); }
        activeCanvas = ArmoryCanvas.GetComponentInChildren<CanvasGroup>();

        scrollContentTransform.DOAnchorPosX(-ScaleManager.Width * 0.5f, 0.5f);

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

        if (_currentPanel != null) { _currentPanel.FadeOut(this); }
        activeCanvas = LevelCanvas.GetComponentInChildren<CanvasGroup>();

        scrollContentTransform.DOAnchorPosX(-ScaleManager.Width * 1.5f, 0.5f);

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

        if (_currentPanel != null) { _currentPanel.FadeOut(this); }
        activeCanvas = ShopCanvas.GetComponentInChildren<CanvasGroup>();

        scrollContentTransform.DOAnchorPosX(-ScaleManager.Width * 2.5f, 0.5f);

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