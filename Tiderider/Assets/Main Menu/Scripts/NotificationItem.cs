using UnityEngine;
using TMPro;
using System.Collections;

public class NotificationItem : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    private CanvasGroup canvasGroup;

    public void Setup(string message)
    {
        messageText.text = message;
        canvasGroup = GetComponent<CanvasGroup>();

        float activeCount = transform.parent.childCount;
        float stayDuration = Mathf.Max(0.5f, 3.0f - (activeCount * 0.4f));

        StartCoroutine(NotificationSequence(stayDuration));
    }

    private IEnumerator NotificationSequence(float stayDuration)
    {
        yield return canvasGroup.Fade(1f, 0.2f, true);

        yield return new WaitForSeconds(stayDuration);

        yield return canvasGroup.Fade(0f, 0.5f, true);

        Destroy(gameObject);
    }
}