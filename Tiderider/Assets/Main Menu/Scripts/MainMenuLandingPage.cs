using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuLandingPage : MonoBehaviour
{
    private readonly struct HiddenObjectState
    {
        public HiddenObjectState(GameObject gameObject, bool wasActive)
        {
            GameObject = gameObject;
            WasActive = wasActive;
        }

        public GameObject GameObject { get; }
        public bool WasActive { get; }
    }

    private const string MainMenuSceneName = "MainMenu";
    private static MainMenuLandingPage instance;

    private readonly List<HiddenObjectState> hiddenObjects = new List<HiddenObjectState>();

    private GameObject landingRoot;
    private CanvasGroup landingCanvasGroup;
    private GameObject settingsPopup;
    private GameObject creditsPopup;
    private bool isTransitioning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        if (instance != null)
            return;

        GameObject bootstrap = new GameObject(nameof(MainMenuLandingPage));
        DontDestroyOnLoad(bootstrap);
        instance = bootstrap.AddComponent<MainMenuLandingPage>();
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
        ClearLanding();

        if (scene.name == MainMenuSceneName)
            StartCoroutine(BuildLandingAfterFrame());
    }

    private IEnumerator BuildLandingAfterFrame()
    {
        yield return null;
        yield return null;

        if (SceneManager.GetActiveScene().name != MainMenuSceneName)
            yield break;

        CentralUIController.Instance?.OpenMainMenu();
        HideUtilityUI();
        CreateLandingUI();
    }

    private void HideUtilityUI()
    {
        hiddenObjects.Clear();

        string[] namesToHide =
        {
            "TopCanvas",
            "BottomCanvas",
            "IconPanel",
            "PlayGamesBtn",
            "PlayGames"
        };

        HashSet<GameObject> seen = new HashSet<GameObject>();
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();

        foreach (string targetName in namesToHide)
        {
            foreach (GameObject root in roots)
            {
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name != targetName)
                        continue;

                    GameObject target = child.gameObject;
                    if (!seen.Add(target))
                        continue;

                    hiddenObjects.Add(new HiddenObjectState(target, target.activeSelf));
                    target.SetActive(false);
                }
            }
        }
    }

    private void RestoreHiddenUI()
    {
        foreach (HiddenObjectState hiddenObject in hiddenObjects)
        {
            if (hiddenObject.GameObject != null)
                hiddenObject.GameObject.SetActive(hiddenObject.WasActive);
        }

        hiddenObjects.Clear();
    }

    private void CreateLandingUI()
    {
        if (landingRoot != null)
            return;

        landingRoot = new GameObject("LandingCanvas");
        Canvas canvas = landingRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        CanvasScaler scaler = landingRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        landingRoot.AddComponent<GraphicRaycaster>();
        landingCanvasGroup = landingRoot.AddComponent<CanvasGroup>();
        DontDestroyOnLoad(landingRoot);

        RectTransform rootRect = landingRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        CreateBackdrop(rootRect);
        RectTransform panel = CreatePanel(rootRect);

        CreateTitle(panel);
        CreateButtonStack(panel);
        CreateSettingsPopup(rootRect);
        CreateCreditsPopup(rootRect);
    }

    private static void CreateBackdrop(RectTransform parent)
    {
        CreateImage(
            "Scrim",
            parent,
            new Color(0.03f, 0.06f, 0.09f, 0.68f),
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.one
        );

        CreateImage(
            "BottomTint",
            parent,
            new Color(0.11f, 0.25f, 0.31f, 0.16f),
            new Vector2(0f, 140f),
            new Vector2(0f, 280f),
            new Vector2(0f, 0f),
            new Vector2(1f, 0f)
        );
    }

    private static RectTransform CreatePanel(RectTransform parent)
    {
        RectTransform panel = CreateImage(
            "ContentPanel",
            parent,
            new Color(0.28f, 0.22f, 0.1f, 0.9f),
            new Vector2(0f, 90f),
            new Vector2(760f, 520f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f)
        );

        Outline outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.82f, 0.68f, 0.38f, 0.35f);
        outline.effectDistance = new Vector2(3f, -3f);

        Shadow shadow = panel.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.45f);
        shadow.effectDistance = new Vector2(0f, -22f);

        return panel;
    }

    private static void CreateTitle(RectTransform parent)
    {
        TMP_Text text = CreateText(
            "LandingTitle",
            parent,
            "TideRider",
            130f,
            FontStyles.Bold,
            new Color(0.98f, 0.96f, 0.91f, 1f),
            TextAlignmentOptions.Center
        );

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.offsetMin = new Vector2(48f, -210f);
        rect.offsetMax = new Vector2(-48f, -48f);
        text.enableWordWrapping = false;
        text.characterSpacing = 1f;
    }

    private void CreateButtonStack(RectTransform parent)
    {
        CreateButton(
            parent,
            "PlayButton",
            "Play",
            new Vector2(0f, -20f),
            new Color(0.82f, 0.6f, 0.22f, 1f),
            new Color(0.17f, 0.11f, 0.04f, 1f),
            BeginReveal
        );

        CreateButton(
            parent,
            "SettingsButton",
            "Settings",
            new Vector2(0f, -150f),
            new Color(0.18f, 0.22f, 0.28f, 0.95f),
            new Color(0.93f, 0.91f, 0.84f, 1f),
            ShowSettingsPopup
        );
    }

    private void CreateButton(
        RectTransform parent,
        string name,
        string labelText,
        Vector2 anchoredPosition,
        Color fillColor,
        Color labelColor,
        UnityAction onClick
    )
    {
        RectTransform buttonRect = CreateImage(
            name,
            parent,
            fillColor,
            anchoredPosition,
            new Vector2(460f, 96f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f)
        );

        Button button = buttonRect.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.96f, 0.9f, 1f);
        colors.pressedColor = new Color(0.92f, 0.82f, 0.65f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(1f, 1f, 1f, 0.45f);
        button.colors = colors;
        button.onClick.AddListener(onClick);

        Outline outline = buttonRect.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.22f, 0.15f, 0.06f, 0.45f);
        outline.effectDistance = new Vector2(3f, -3f);

        TMP_Text label = CreateText(
            "ButtonLabel",
            buttonRect,
            labelText,
            50f,
            FontStyles.Bold,
            labelColor,
            TextAlignmentOptions.Center
        );

        RectTransform rect = label.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void CreateSettingsPopup(RectTransform root)
    {
        RectTransform popup = CreateImage(
            "SettingsPopup",
            root,
            new Color(0.08f, 0.1f, 0.13f, 1f),
            new Vector2(0f, -60f),
            new Vector2(760f, 980f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f)
        );
        settingsPopup = popup.gameObject;
        settingsPopup.SetActive(false);

        Outline outline = popup.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.82f, 0.68f, 0.38f, 0.35f);
        outline.effectDistance = new Vector2(3f, -3f);

        TMP_Text title = CreateText(
            "SettingsTitle",
            popup,
            "Settings",
            74f,
            FontStyles.Bold,
            new Color(0.98f, 0.96f, 0.91f, 1f),
            TextAlignmentOptions.Center
        );
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(40f, -150f);
        titleRect.offsetMax = new Vector2(-40f, -40f);

        CreateSliderRow(popup, "Music", "MusicVolume", new Vector2(0f, 200f));
        CreateSliderRow(popup, "SFX", "SFXVolume", new Vector2(0f, 70f));

        CreateButton(
            popup,
            "CreditsButton",
            "Credits",
            new Vector2(0f, -120f),
            new Color(0.18f, 0.22f, 0.28f, 0.95f),
            new Color(0.93f, 0.91f, 0.84f, 1f),
            ShowCreditsPopup
        );

        CreateButton(
            popup,
            "RestoreButton",
            "Restore",
            new Vector2(0f, -260f),
            new Color(0.18f, 0.22f, 0.28f, 0.95f),
            new Color(0.93f, 0.91f, 0.84f, 1f),
            TriggerRestorePurchases
        );

        CreateButton(
            popup,
            "BackSettingsButton",
            "Back",
            new Vector2(0f, -400f),
            new Color(0.82f, 0.6f, 0.22f, 1f),
            new Color(0.17f, 0.11f, 0.04f, 1f),
            HideSettingsPopup
        );
    }

    private void CreateCreditsPopup(RectTransform root)
    {
        RectTransform popup = CreateImage(
            "CreditsPopup",
            root,
            new Color(0.08f, 0.1f, 0.13f, 0.96f),
            new Vector2(0f, -60f),
            new Vector2(760f, 980f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f)
        );
        creditsPopup = popup.gameObject;
        creditsPopup.SetActive(false);

        Outline outline = popup.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.82f, 0.68f, 0.38f, 0.35f);
        outline.effectDistance = new Vector2(3f, -3f);

        TMP_Text title = CreateText(
            "CreditsPopupTitle",
            popup,
            "Credits",
            74f,
            FontStyles.Bold,
            new Color(0.98f, 0.96f, 0.91f, 1f),
            TextAlignmentOptions.Center
        );
        RectTransform titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(40f, -150f);
        titleRect.offsetMax = new Vector2(-40f, -40f);

        TMP_Text body = CreateText(
            "CreditsBody",
            popup,
            BuildCreditsText(),
            34f,
            FontStyles.Normal,
            new Color(0.93f, 0.91f, 0.84f, 1f),
            TextAlignmentOptions.TopLeft
        );
        RectTransform bodyRect = body.rectTransform;
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.offsetMin = new Vector2(50f, 160f);
        bodyRect.offsetMax = new Vector2(-50f, -170f);
        body.enableWordWrapping = true;
        body.lineSpacing = -6f;

        CreateButton(
            popup,
            "BackCreditsButton",
            "Back",
            new Vector2(0f, -390f),
            new Color(0.82f, 0.6f, 0.22f, 1f),
            new Color(0.17f, 0.11f, 0.04f, 1f),
            HideCreditsPopup
        );
    }

    private void CreateSliderRow(RectTransform parent, string labelText, string playerPrefsKey, Vector2 anchoredPosition)
    {
        TMP_Text label = CreateText(
            $"{labelText}Label",
            parent,
            labelText,
            46f,
            FontStyles.Bold,
            new Color(0.93f, 0.91f, 0.84f, 1f),
            TextAlignmentOptions.Left
        );
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = anchoredPosition + new Vector2(-255f, 0f);
        labelRect.sizeDelta = new Vector2(150f, 60f);

        GameObject sliderObject = new GameObject(
            $"{labelText}Slider",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Slider)
        );
        sliderObject.transform.SetParent(parent, false);

        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = anchoredPosition + new Vector2(80f, 0f);
        sliderRect.sizeDelta = new Vector2(360f, 40f);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = PlayerPrefs.GetFloat(playerPrefsKey, 1f);

        RectTransform background = CreateImage(
            "Background",
            sliderRect,
            new Color(0.18f, 0.22f, 0.28f, 1f),
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.one
        );

        RectTransform fillArea = new GameObject("Fill Area", typeof(RectTransform)).GetComponent<RectTransform>();
        fillArea.SetParent(sliderRect, false);
        fillArea.anchorMin = new Vector2(0f, 0f);
        fillArea.anchorMax = new Vector2(1f, 1f);
        fillArea.offsetMin = new Vector2(10f, 10f);
        fillArea.offsetMax = new Vector2(-10f, -10f);

        RectTransform fill = CreateImage(
            "Fill",
            fillArea,
            new Color(0.82f, 0.6f, 0.22f, 1f),
            Vector2.zero,
            Vector2.zero,
            Vector2.zero,
            Vector2.one
        );

        RectTransform handleArea = new GameObject("Handle Slide Area", typeof(RectTransform)).GetComponent<RectTransform>();
        handleArea.SetParent(sliderRect, false);
        handleArea.anchorMin = new Vector2(0f, 0f);
        handleArea.anchorMax = new Vector2(1f, 1f);
        handleArea.offsetMin = Vector2.zero;
        handleArea.offsetMax = Vector2.zero;

        RectTransform handle = CreateImage(
            "Handle",
            handleArea,
            new Color(0.98f, 0.96f, 0.91f, 1f),
            Vector2.zero,
            new Vector2(32f, 56f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f)
        );

        slider.targetGraphic = handle.GetComponent<Image>();
        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.onValueChanged.AddListener(value =>
        {
            PlayerPrefs.SetFloat(playerPrefsKey, value);
            PlayerPrefs.Save();
        });
    }

    private void BeginReveal()
    {
        if (!isTransitioning)
            StartCoroutine(RevealMenuRoutine());
    }

    private void ShowSettingsPopup()
    {
        if (settingsPopup != null)
            settingsPopup.SetActive(true);
    }

    private void HideSettingsPopup()
    {
        if (settingsPopup != null)
            settingsPopup.SetActive(false);
    }

    private void ShowCreditsPopup()
    {
        if (settingsPopup != null)
            settingsPopup.SetActive(false);

        if (creditsPopup != null)
            creditsPopup.SetActive(true);
    }

    private void HideCreditsPopup()
    {
        if (creditsPopup != null)
            creditsPopup.SetActive(false);

        if (settingsPopup != null)
            settingsPopup.SetActive(true);
    }

    private void TriggerRestorePurchases()
    {
        Button restoreButton = FindButtonByName("RestoreBtn");
        if (restoreButton != null)
        {
            restoreButton.onClick.Invoke();
            return;
        }

        Debug.LogWarning("Restore button was not found in MainMenu scene.");
    }

    private IEnumerator RevealMenuRoutine()
    {
        isTransitioning = true;

        RestoreHiddenUI();
        CentralUIController.Instance?.OpenMainMenu();

        if (landingCanvasGroup != null)
        {
            const float duration = 0.3f;
            float elapsed = 0f;
            float startAlpha = landingCanvasGroup.alpha;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                landingCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                yield return null;
            }

            landingCanvasGroup.alpha = 0f;
        }

        ClearLanding();
        isTransitioning = false;
    }

    private void ClearLanding()
    {
        if (landingRoot != null)
            Destroy(landingRoot);

        landingRoot = null;
        landingCanvasGroup = null;
        settingsPopup = null;
        creditsPopup = null;
        isTransitioning = false;
    }

    private static string BuildCreditsText()
    {
        HashSet<string> seen = new HashSet<string>();
        List<string> lines = new List<string>();

        CollectTextFromObjectNamed("CreditsPanel", seen, lines);
        CollectTextFromObjectNamed("CreditsMenu", seen, lines);

        if (lines.Count == 0)
            return "Credits are available in the main menu settings panel.";

        return string.Join("\n", lines);
    }

    private static void CollectTextFromObjectNamed(string objectName, HashSet<string> seen, List<string> lines)
    {
        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name != objectName)
                    continue;

                foreach (TMP_Text tmpText in child.GetComponentsInChildren<TMP_Text>(true))
                    AddCreditLine(tmpText.text, seen, lines);

                foreach (Text uiText in child.GetComponentsInChildren<Text>(true))
                    AddCreditLine(uiText.text, seen, lines);
            }
        }
    }

    private static void AddCreditLine(string value, HashSet<string> seen, List<string> lines)
    {
        string text = value?.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (text.Equals("Credits", System.StringComparison.OrdinalIgnoreCase))
            return;

        if (text.Equals("Back", System.StringComparison.OrdinalIgnoreCase))
            return;

        if (seen.Add(text))
            lines.Add(text);
    }

    private static Button FindButtonByName(string buttonName)
    {
        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name != buttonName)
                    continue;

                Button button = child.GetComponent<Button>();
                if (button != null)
                    return button;
            }
        }

        return null;
    }

    private static RectTransform CreateImage(
        string name,
        RectTransform parent,
        Color color,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Vector2 anchorMin,
        Vector2 anchorMax
    )
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        gameObject.transform.SetParent(parent, false);

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        Image image = gameObject.GetComponent<Image>();
        image.color = color;

        return rectTransform;
    }

    private static TMP_Text CreateText(
        string name,
        RectTransform parent,
        string content,
        float fontSize,
        FontStyles fontStyle,
        Color color,
        TextAlignmentOptions alignment
    )
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        gameObject.transform.SetParent(parent, false);

        TMP_Text text = gameObject.GetComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(24f, fontSize * 0.45f);
        text.fontSizeMax = fontSize;

        return text;
    }
}
