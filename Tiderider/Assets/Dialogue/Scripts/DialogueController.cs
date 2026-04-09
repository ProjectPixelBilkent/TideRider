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
    [SerializeField] private Vector2 leftShownPosition = new Vector2(-90f, -1050f);
    [SerializeField] private Vector2 rightShownPosition = new Vector2(110f, -1050f);
    [SerializeField] private Vector2 leftHiddenPosition = new Vector2(-1200f, -1050f);
    [SerializeField] private Vector2 rightHiddenPosition = new Vector2(1200f, -1050f);

    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float typingSpeed = 0.03f;

    public IEnumerator HideDialogue()
    {
        Vector2 currentPosition = dialogueFrameTransform.anchoredPosition;
        Vector2 hiddenPosition = currentPosition.x < 0 ? leftHiddenPosition : rightHiddenPosition;
        yield return StartCoroutine(Slide(dialogueFrameTransform, currentPosition, hiddenPosition));
        dialogueText.text = "";
    }
    private void Start()
    {
        dialogueFrameTransform.anchoredPosition = leftHiddenPosition;
        dialogueText.text = "";
    }
    public IEnumerator PlayDialogueLine(DialogueLineData data, Sprite frameSprite)
    {

        if (dialogueFrameTransform == null || dialogueFrameImage == null || dialogueText == null)
        {
            Debug.LogError("DialogueController references are not assigned in the Inspector.");
            yield break;
        }

        dialogueFrameImage.sprite = frameSprite;

        Vector2 shownPosition;
        Vector2 hiddenPosition;

        if (data.enterSide == DialogueSide.Left)
        {
            shownPosition = leftShownPosition;
            hiddenPosition = leftHiddenPosition;
        }
        else
        {
            shownPosition = rightShownPosition;
            hiddenPosition = rightHiddenPosition;
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
        dialogueText.text = sentence;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int totalCharacters = dialogueText.textInfo.characterCount;

        for (int i = 0; i <= totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}