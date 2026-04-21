using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public class GlobalUIFontBootstrap : MonoBehaviour
{
    private const string ConfigResourcePath = "GlobalUIFontConfig";
    private const float RefreshIntervalSeconds = 0.5f;
    private static readonly char[] TurkishCharacters = { '\u00C7', '\u00E7', '\u011E', '\u011F', '\u0130', '\u0131', '\u00D6', '\u00F6', '\u015E', '\u015F', '\u00DC', '\u00FC' };

    private static GlobalUIFontBootstrap instance;
    private static TMP_FontAsset runtimeFontAsset;
    private static TMP_FontAsset fallbackFontAsset;
    private static Font unityFont;
    private static bool initialized;

    private float nextRefreshTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        if (instance != null)
            return;

        GameObject bootstrapObject = new GameObject(nameof(GlobalUIFontBootstrap));
        DontDestroyOnLoad(bootstrapObject);
        instance = bootstrapObject.AddComponent<GlobalUIFontBootstrap>();
    }

    private void Awake()
    {
        if (initialized)
            return;

        if (!TryCreateRuntimeFont())
        {
            enabled = false;
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyToLoadedText();
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || Time.unscaledTime < nextRefreshTime)
            return;

        nextRefreshTime = Time.unscaledTime + RefreshIntervalSeconds;
        ApplyToLoadedText();
    }

    private void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToLoadedText();
    }

    private static bool TryCreateRuntimeFont()
    {
        GlobalUIFontConfig config = Resources.Load<GlobalUIFontConfig>(ConfigResourcePath);
        if (config == null || config.sourceFont == null)
        {
            Debug.LogError(
                $"Global UI font config was not found at Resources/{ConfigResourcePath} or has no source font assigned."
            );
            return false;
        }

        unityFont = config.sourceFont;

        fallbackFontAsset = TMP_Settings.defaultFontAsset;
        runtimeFontAsset = TMP_FontAsset.CreateFontAsset(
            config.sourceFont,
            config.samplingPointSize,
            config.atlasPadding,
            GlyphRenderMode.SDFAA,
            config.atlasWidth,
            config.atlasHeight,
            AtlasPopulationMode.Dynamic,
            true
        );

        runtimeFontAsset.name = $"{config.sourceFont.name} Runtime SDF";

        if (fallbackFontAsset != null && fallbackFontAsset != runtimeFontAsset)
        {
            runtimeFontAsset.fallbackFontAssetTable = new List<TMP_FontAsset> { fallbackFontAsset };
        }

        TMP_Settings.defaultFontAsset = runtimeFontAsset;
        return true;
    }

    private static void ApplyToLoadedText()
    {
        if (runtimeFontAsset == null || unityFont == null)
            return;

        TMP_Text[] tmpTexts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_Text tmpText in tmpTexts)
        {
            if (tmpText == null)
                continue;

            if (tmpText.GetComponentInParent<ExcludeFromGlobalUIFontSwap>(true) != null)
                continue;

            TMP_FontAsset targetFont = ShouldUseFallbackFont(tmpText.text) && fallbackFontAsset != null
                ? fallbackFontAsset
                : runtimeFontAsset;

            if (tmpText.font != targetFont)
                tmpText.font = targetFont;

            tmpText.SetMaterialDirty();
            tmpText.SetVerticesDirty();
        }

        Text[] legacyTexts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Text legacyText in legacyTexts)
        {
            if (legacyText == null)
                continue;

            if (legacyText.font != unityFont)
                legacyText.font = unityFont;

            legacyText.SetAllDirty();
        }
    }

    private static bool ShouldUseFallbackFont(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        return text.IndexOfAny(TurkishCharacters) >= 0;
    }

    public static TMP_FontAsset GetFallbackFontAsset()
    {
        return fallbackFontAsset;
    }
}
