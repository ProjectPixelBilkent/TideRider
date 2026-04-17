using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform dialogueFrameTransform;
    [SerializeField] private Image dialogueFrameImage;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Frame Position Settings")]
    [SerializeField] private Vector2 leftShownPosition = new Vector2(-75f, -1050f);
    [SerializeField] private Vector2 rightShownPosition = new Vector2(75f, -1050f);
    [SerializeField] private Vector2 middleShownPosition = new Vector2(0f, -1050f);

    [SerializeField] private Vector2 leftHiddenPosition = new Vector2(-1200f, -1050f);
    [SerializeField] private Vector2 rightHiddenPosition = new Vector2(1200f, -1050f);
    [SerializeField] private Vector2 middleHiddenPosition = new Vector2(0f, -1400f);

    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float typingSpeed = 0.03f;

    public bool IsTyping { get; private set; }
    private bool _skipTyping = false;

    public void ConfigureReferences(RectTransform frameTransform, Image frameImage, TMP_Text text)
    {
        dialogueFrameTransform = frameTransform;
        dialogueFrameImage = frameImage;
        dialogueText = text;
    }

    public void ConfigurePositions(
        Vector2 leftShown,
        Vector2 rightShown,
        Vector2 middleShown,
        Vector2 leftHidden,
        Vector2 rightHidden,
        Vector2 middleHidden)
    {
        leftShownPosition = leftShown;
        rightShownPosition = rightShown;
        middleShownPosition = middleShown;
        leftHiddenPosition = leftHidden;
        rightHiddenPosition = rightHidden;
        middleHiddenPosition = middleHidden;
    }

    public void SkipTyping() { _skipTyping = true; }

    private void Start()
    {
        if (dialogueFrameTransform != null)
        {
            dialogueFrameTransform.anchoredPosition = middleHiddenPosition;
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialogueFrameImage != null)
        {
            dialogueFrameImage.preserveAspect = true;
            dialogueFrameImage.type = Image.Type.Simple;
        }
    }

    public IEnumerator HideDialogue()
    {
        if (dialogueFrameTransform == null || dialogueText == null)
        {
            Debug.LogError("DialogueController references are not assigned in the Inspector.");
            yield break;
        }

        Vector2 currentPosition = dialogueFrameTransform.anchoredPosition;
        Vector2 hiddenPosition;

        if (ApproximatelySamePosition(currentPosition, leftShownPosition))
        {
            hiddenPosition = leftHiddenPosition;
        }
        else if (ApproximatelySamePosition(currentPosition, middleShownPosition))
        {
            hiddenPosition = middleHiddenPosition;
        }
        else
        {
            hiddenPosition = rightHiddenPosition;
        }

        yield return StartCoroutine(Slide(dialogueFrameTransform, currentPosition, hiddenPosition));
        dialogueText.text = "";
    }

    public IEnumerator PlayDialogueLine(DialogueLineData data, Sprite frameSprite)
    {
        if (dialogueFrameTransform == null || dialogueFrameImage == null || dialogueText == null)
        {
            Debug.LogError("DialogueController references are not assigned in the Inspector.");
            yield break;
        }

        if (frameSprite == null)
        {
            Debug.LogError("Frame sprite is null.");
            yield break;
        }

        dialogueText.text = "";
        dialogueText.maxVisibleCharacters = 0;
        dialogueFrameImage.sprite = frameSprite;
        dialogueFrameImage.preserveAspect = true;
        dialogueFrameImage.type = Image.Type.Simple;

        Vector2 shownPosition;
        Vector2 hiddenPosition;

        switch (data.enterSide)
        {
            case DialogueSide.Left:
                shownPosition = leftShownPosition;
                hiddenPosition = leftHiddenPosition;
                break;

            case DialogueSide.Middle:
                shownPosition = middleShownPosition;
                hiddenPosition = middleHiddenPosition;
                break;

            case DialogueSide.Right:
            default:
                shownPosition = rightShownPosition;
                hiddenPosition = rightHiddenPosition;
                break;
        }

        dialogueFrameTransform.anchoredPosition = hiddenPosition;

        yield return StartCoroutine(Slide(dialogueFrameTransform, hiddenPosition, shownPosition));
        yield return StartCoroutine(TypeText(data.text));
    }

    private IEnumerator Slide(RectTransform target, Vector2 from, Vector2 to)
    {
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            target.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }

        target.anchoredPosition = to;
    }

    private IEnumerator TypeText(string sentence)
    {
        _skipTyping = false;
        IsTyping = true;
        dialogueText.text = sentence;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int totalCharacters = dialogueText.textInfo.characterCount;

        for (int i = 0; i <= totalCharacters; i++)
        {
            if (_skipTyping)
            {
                dialogueText.maxVisibleCharacters = totalCharacters;
                break;
            }
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }

        IsTyping = false;
    }

    private bool ApproximatelySamePosition(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b) < 5f;
    }
}
