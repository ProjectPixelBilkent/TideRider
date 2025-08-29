using System;
using System.Collections;
using UnityEngine;
using TMPro;

[Serializable]
public class DialogueFile {
    public DialogueLine[] lines;
}

[Serializable]
public class DialogueLine {
    public string speaker;
    public string text;
    public float wait = 0f; // autoAdvance açıksa bu kadar bekleyip geçer
}

public class DialogueRunner : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI bodyText;

    [Header("Data")]
    [Tooltip("Resources altındaki yol (uzantısız). Örn: dialogues/intro")]
    public string resourcePath = "dialogues/intro";

    [Header("Behavior")]
    [Tooltip("Karakter/saniye (typewriter)")]
    public float charsPerSecond = 40f;
    [Tooltip("Otomatik ilerlesin mi? (true ise wait süresine göre)")]
    public bool autoAdvance = false;

    // dokunuş debouncing
    private float _lastTapTime = -999f;
    private const float TapCooldown = 0.08f; // çok hızlı çift dokunuşları filtrele

    private DialogueFile _dialogue;
    private int _index = -1;
    private bool _typing = false;
    private Coroutine _typingCo;

    void Start() {
        LoadAndStart();
    }

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

        // EDITOR/PC: sol tık veya Space
        bool mouseTap = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);

        // MOBİL: ekrana ilk dokunuş
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
        // İstersen UI'yı temizleyebilirsin:
        // if (speakerText) speakerText.text = "";
        // if (bodyText) bodyText.text = "";
    }
}