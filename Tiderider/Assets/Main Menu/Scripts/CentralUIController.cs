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

    // --- CACHED REFERENCES ---
    private RectTransform[] navIcons;
    private Image[] navImages;

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

        navIcons = new RectTransform[3];
        navImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            navIcons[i] = IconPanel.transform.GetChild(i).GetComponent<RectTransform>();
            navImages[i] = navIcons[i].GetComponent<Image>();
        }
    }

    /// <summary>
    /// Toggles the visibility of a panel using a fade transition.
    /// </summary>
    /// <remarks>Maintained by: Işık Dönger</remarks>
    public void TogglePanel(CanvasGroup targetPanel)
    {
        if (_activeTransition != null)
            StopCoroutine(_activeTransition);

        _activeTransition = StartCoroutine(TogglePanelRoutine(targetPanel, activeCanvas));
    }

    /// <summary>
    /// Coroutine that handles the fading in/out transitions for panel toggling.
    /// </summary>
    private IEnumerator TogglePanelRoutine(CanvasGroup target, CanvasGroup backgroundToRestore)
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

            yield return backgroundToRestore.FadeIn(this);
        }
        else
        {
            yield return backgroundToRestore.FadeOut(this);
            yield return target.FadeIn(this);
            _currentPanel = target;
        }

        _activeTransition = null;
    }

    /// <summary>
    /// Manages menu changes for scrolling and updates the icon panel accordingly.
    /// </summary>
    /// <remarks>Maintained by: Işık Dönger</remarks>
    public void ChangeMenu()
    {
        float xPos = scrollContentTransform.anchoredPosition.x;

        if (xPos > -ScaleManager.Width) OpenArmory();
        else if (xPos > -ScaleManager.Width * 2) OpenMainMenu();
        else OpenShop();
    }

    public void OpenArmory() => SwitchTab(0, ArmoryCanvas, -ScaleManager.Width * 0.5f);
    public void OpenMainMenu() => SwitchTab(1, LevelCanvas, -ScaleManager.Width * 1.5f);
    public void OpenShop() => SwitchTab(2, ShopCanvas, -ScaleManager.Width * 2.5f);

    /// <summary>
    /// Handles the heavy lifting of killing tweens, updating canvases, and animating the nav bar.
    /// </summary>
    private void SwitchTab(int selectedIndex, RectTransform targetCanvas, float targetScrollX)
    {
        if (_currentPanel != null)
        {
            TogglePanel(_currentPanel);
        }

        if (activeCanvas != null)
        {
            activeCanvas.alpha = 1f;
            activeCanvas.blocksRaycasts = true;
            activeCanvas.interactable = true;
        }

        activeCanvas = targetCanvas.GetComponentInChildren<CanvasGroup>();

        scrollContentTransform.DOKill();
        scrollContentTransform.DOAnchorPosX(targetScrollX, 0.5f);

        // Animate Icons
        for (int i = 0; i < 3; i++)
        {
            navIcons[i].DOKill();
            navImages[i].DOKill();

            bool isSelected = (i == selectedIndex);
            float targetWidth = isSelected ? ScaleManager.SelectedIconWidth : ScaleManager.SideIconWidth;
            Color targetColor = isSelected ? ScaleManager.SelectedColor : ScaleManager.SideColor;

            navIcons[i].DOSizeDelta(new Vector2(targetWidth, navIcons[i].sizeDelta.y), 0.5f);
            navImages[i].DOColor(targetColor, 0.5f);

            float targetPosX = GetIconPositionX(i, selectedIndex);
            navIcons[i].DOAnchorPosX(targetPosX, 0.5f);
        }
    }

    /// <summary>
    /// Returns the precise mathematical X position for an icon based on the active tab.
    /// </summary>
    private float GetIconPositionX(int iconIndex, int selectedIndex)
    {
        float selWidth = ScaleManager.SelectedIconWidth;
        float sideWidth = ScaleManager.SideIconWidth;

        if (selectedIndex == 0) // Armory
        {
            if (iconIndex == 0) return selWidth / 2f;
            if (iconIndex == 1) return (selWidth - sideWidth) / 2f;
            return -sideWidth / 2f;
        }
        if (selectedIndex == 1) // Main Menu
        {
            if (iconIndex == 0) return sideWidth / 2f;
            if (iconIndex == 1) return 0f;
            return -sideWidth / 2f;
        }
        else // Shop
        {
            if (iconIndex == 0) return sideWidth / 2f;
            if (iconIndex == 1) return -(selWidth - sideWidth) / 2f;
            return -selWidth / 2f;
        }
    }
}