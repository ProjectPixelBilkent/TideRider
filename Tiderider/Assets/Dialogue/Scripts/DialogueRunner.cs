using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class DialogueFile {
    public DialogueLine[] lines;
}

[Serializable]
public class DialogueLine {
    public string speaker;   // "NPC", "Hero", "Narrator"...
    public string text;      // gösterilecek metin
    public float wait = 0f;  // autoAdvance açıksa bu kadar bekleyip geçer
    public string portrait;  // Resources içinden uzantısız yol: "Portraits/npc1"
}

public class DialogueRunner : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI speakerText;   // üstte küçük isim
    public TextMeshProUGUI bodyText;      // alttaki diyalog metni
    public Image portraitImage;           // metin kutusuna komşu küçük resim

    [Header("Data")]
    [Tooltip("Resources altındaki yol (uzantısız). Örn: Dialogues/intro")]
    public string resourcePath = "Dialogues/intro";

    [Header("Behavior")]
    [Tooltip("Karakter/saniye (typewriter hızı)")]
    public float charsPerSecond = 40f;
    [Tooltip("Satırlar otomatik ilerlesin mi? true ise 'wait' kadar bekler")]
    public bool autoAdvance = false;

    // dokunma debouncing
    private float _lastTapTime = -999f;
    private const float TapCooldown = 0.08f;

    // dahili durum
    private DialogueFile _dialogue;
    private int _index = -1;
    private bool _typing = false;
    private Coroutine _typingCo;

    // sprite cache
    private readonly Dictionary<string, Sprite> _spriteCache = new();

    void Start() {
        LoadAndStart();
    }

    /// <summary>JSON'u Resources'tan yükler ve diyalogu başlatır.</summary>
    public void LoadAndStart() {
        var ta = Resources.Load<TextAsset>(resourcePath);
        if (ta == null) {
            Debug.LogError($"Dialogue JSON not found at Resources/{resourcePath}.json");
            return;
        }
        _dialogue = JsonUtility.FromJson<DialogueFile>(ta.text);
        if (_dialogue == null || _dialogue.lines == null || _dialogue.lines.Length == 0) {
            Debug.LogError("Dialogue JSON is empty or invalid.");
            return;
        }
        _index = -1;
        Next();
    }

    void Update() {
        if (_dialogue == null) return;

        // Editor/PC
        bool mouseTap = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);

        // Mobil dokunuş
        bool touchTap = false;
        if (Input.touchCount > 0) {
            var t = Input.GetTouch(0);
            touchTap = (t.phase == TouchPhase.Began);
        }

        if ((mouseTap || touchTap) && Time.time - _lastTapTime > TapCooldown) {
            _lastTapTime = Time.time;
            OnAdvanceInput();
        }
    }

    private void OnAdvanceInput() {
        if (_dialogue == null) return;

        if (_typing) {
            CompleteTyping();   // yazıyı anında tamamla
        } else {
            Next();             // bir sonraki satıra geç
        }
    }

    private void Next() {
        _index++;
        if (_index >= _dialogue.lines.Length) {
            OnEnd();
            return;
        }

        var line = _dialogue.lines[_index];

        // Portreyi uygula (varsa)
        ApplyPortrait(line);

        // Typewriter başlat
        if (_typingCo != null) StopCoroutine(_typingCo);
        _typingCo = StartCoroutine(TypeLine(line));
    }

    private IEnumerator TypeLine(DialogueLine line) {
        _typing = true;

        if (speakerText) speakerText.text = string.IsNullOrEmpty(line.speaker) ? "" : line.speaker;
        if (bodyText) bodyText.text = "";

        string s = line.text ?? "";
        float delay = (charsPerSecond <= 0f) ? 0f : 1f / charsPerSecond;

        foreach (char ch in s) {
            bodyText.text += ch;
            if (delay > 0f) yield return new WaitForSeconds(delay);
            else yield return null; // bir frame bekle
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

    private void OnEnd() {
        Debug.Log("Dialogue finished.");
        // İstersen UI temizliği:
        // if (speakerText) speakerText.text = "";
        // if (bodyText) bodyText.text = "";
        // if (portraitImage) portraitImage.enabled = false;
    }

    // --- Helpers ---

    private void ApplyPortrait(DialogueLine line) {
        if (!portraitImage) return;

        if (!string.IsNullOrEmpty(line.portrait)) {
            var sp = LoadSprite(line.portrait);
            portraitImage.sprite = sp;
            portraitImage.enabled = sp != null;
            portraitImage.preserveAspect = true;
        } else {
            // Satırda portre belirtilmemişse gizle (yalnızca anlatıcı metni vs.)
            portraitImage.enabled = false;
        }
    }

    private Sprite LoadSprite(string resPath) {
        if (string.IsNullOrEmpty(resPath)) return null;
        if (_spriteCache.TryGetValue(resPath, out var s)) return s;

        var loaded = Resources.Load<Sprite>(resPath);
        if (loaded == null) {
            Debug.LogWarning($"Sprite not found: Resources/{resPath} (uzantısız path ve Resources içinde olmalı)");
            return null;
        }
        _spriteCache[resPath] = loaded;
        return loaded;
    }
}