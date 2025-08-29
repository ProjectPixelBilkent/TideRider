using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class DialogueFile { public DialogueLine[] lines; }

[Serializable]
public class DialogueLine {
    public string speaker;     // "NPC", "Hero", "Narrator"...
    public string text;        // diyalog metni
    public float wait = 0f;    // autoAdvance açıksa, satır bitince bekleme
    public string portrait;    // Resources yolu: "Portraits/some_sprite" (uzantısız)
}

public class DialogueRunner : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI speakerText;   // Canvas/SpeakerText
    public TextMeshProUGUI bodyText;      // Canvas/PortraitImage/BodyText
    public Image portraitImage;           // Canvas/PortraitImage (Image)

    [Header("Animate THIS (PortraitImage)")]
    public RectTransform dialoguePanel;   // Canvas/PortraitImage (RectTransform)

    [Header("Data")]
    [Tooltip("Resources altındaki JSON yolu (uzantısız). Örn: Dialogues/intro")]
    public string resourcePath = "Dialogues/intro";

    [Header("Behavior")]
    [Tooltip("Karakter/saniye (typewriter hızı)")]
    public float charsPerSecond = 40f;
    [Tooltip("Satırlar otomatik ilerlesin mi? true ise 'wait' kadar bekler")]
    public bool autoAdvance = false;

    [Header("Panel Anim (sadece pozisyon)")]
    [Tooltip("Giriş/çıkış süresi (sn)")]
    public float panelAnimDuration = 0.5f;
    [Tooltip("Dikey kaydırma ofseti (px). Pozitif ise aşağıdan yukarı kayar.")]
    public float slideOffsetY = 200f;
    [Tooltip("Animasyon eğrisi")]
    public AnimationCurve panelCurve = AnimationCurve.EaseInOut(0,0,1,1);

    // input debouncing
    private float _lastTapTime = -999f;
    private const float TapCooldown = 0.08f;

    // state
    private DialogueFile _dialogue;
    private int _index = -1;
    private bool _typing = false;
    private Coroutine _typingCo;
    private bool _panelAnimating = false;

    private Vector2 _homePos;                      // panelin sabit hedef konumu
    private readonly Dictionary<string, Sprite> _spriteCache = new();

    void Start() { LoadAndStart(); }

    public void LoadAndStart() {
        var ta = Resources.Load<TextAsset>(resourcePath);
        if (ta == null) { Debug.LogError($"Dialogue JSON not found: Resources/{resourcePath}.json"); return; }
        _dialogue = JsonUtility.FromJson<DialogueFile>(ta.text);
        if (_dialogue?.lines == null || _dialogue.lines.Length == 0) { Debug.LogError("Dialogue JSON empty/invalid."); return; }

        // Sahnede PortraitImage (dialoguePanel) neredeyse, orası "home" konum.
        _homePos = dialoguePanel.anchoredPosition;

        _index = -1;
        Next();
    }

    void Update() {
        if (_dialogue == null || _panelAnimating) return;

        bool mouseTap = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
        bool touchTap = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;

        if ((mouseTap || touchTap) && Time.time - _lastTapTime > TapCooldown) {
            _lastTapTime = Time.time;
            if (_typing) CompleteTyping(); else Next();
        }
    }

    public void Next() { StartCoroutine(NextRoutine()); }

    private IEnumerator NextRoutine() {
        if (_typing) CompleteTyping();

        // İlk satırda dışarı kaydırma yapma (panel zaten home'da)
        if (_index >= 0) {
            yield return SlideOut();   // home → home - offset
        }

        _index++;
        if (_index >= _dialogue.lines.Length) { Debug.Log("Dialogue finished."); yield break; }

        var line = _dialogue.lines[_index];

        // portre sprite'ını satır bazında değiştir
        ApplyPortraitSprite(line);

        // metinleri hazırla (body boş; typewriter animasyondan sonra başlayacak)
        if (speakerText) speakerText.text = string.IsNullOrEmpty(line.speaker) ? "" : line.speaker;
        if (bodyText) bodyText.text = "";

        // Paneli ekrana geri kaydır (home - offset → home)
        yield return SlideIn();

        // Typewriter başlasın
        if (_typingCo != null) StopCoroutine(_typingCo);
        _typingCo = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(DialogueLine line) {
        _typing = true;
        string s = line.text ?? "";
        float delay = (charsPerSecond <= 0f) ? 0f : 1f / charsPerSecond;

        foreach (char ch in s) {
            bodyText.text += ch;
            if (delay > 0f) yield return new WaitForSeconds(delay);
            else yield return null;
        }

        _typing = false;

        if (autoAdvance) {
            float w = Mathf.Max(0f, line.wait);
            if (w > 0f) yield return new WaitForSeconds(w);
            Next();
        }
    }

    private void CompleteTyping() {
        if (!_typing) return;
        StopCoroutine(_typingCo);
        var line = _dialogue.lines[_index];
        bodyText.text = line.text ?? "";
        _typing = false;
    }

    // --- panel animasyonları (drift yok: sabit _homePos referansı) ---

    private IEnumerator SlideOut() {
        if (!dialoguePanel) yield break;
        _panelAnimating = true;

        // daima home → home - offset
        Vector2 startPos = _homePos;
        Vector2 endPos   = _homePos - new Vector2(0, slideOffsetY);

        // emin olmak için tam home'a oturt
        dialoguePanel.anchoredPosition = startPos;

        float t = 0f;
        while (t < 1f) {
            t += Time.deltaTime / panelAnimDuration;
            float k = panelCurve.Evaluate(Mathf.Clamp01(t));
            dialoguePanel.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, k);
            yield return null;
        }
        _panelAnimating = false;
    }

    private IEnumerator SlideIn() {
        if (!dialoguePanel) yield break;
        _panelAnimating = true;

        // daima home - offset → home
        Vector2 startPos = _homePos - new Vector2(0, slideOffsetY);
        Vector2 endPos   = _homePos;

        // paneli ekran dışında başlat
        dialoguePanel.anchoredPosition = startPos;

        float t = 0f;
        while (t < 1f) {
            t += Time.deltaTime / panelAnimDuration;
            float k = panelCurve.Evaluate(Mathf.Clamp01(t));
            dialoguePanel.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, k);
            yield return null;
        }
        _panelAnimating = false;
    }

    // --- helpers ---

    private void ApplyPortraitSprite(DialogueLine line) {
        if (!portraitImage) return;

        if (!string.IsNullOrEmpty(line.portrait)) {
            var sp = LoadSprite(line.portrait);
            portraitImage.sprite = sp;
            portraitImage.enabled = sp != null;
            portraitImage.preserveAspect = true;
        } else {
            // satırda portrait verilmemişse, var olanı korumak istiyorsan burayı boş bırak.
            // tamamen gizlemek istersen:
            // if (portraitImage.sprite == null) portraitImage.enabled = false;
        }
    }

    private Sprite LoadSprite(string resPath) {
        if (string.IsNullOrEmpty(resPath)) return null;
        if (_spriteCache.TryGetValue(resPath, out var s)) return s;

        var loaded = Resources.Load<Sprite>(resPath);
        if (loaded == null) {
            Debug.LogWarning($"Sprite not found: Resources/{resPath}  (uzantısız path ve Resources içinde olmalı)");
            return null;
        }
        _spriteCache[resPath] = loaded;
        return loaded;
    }
}