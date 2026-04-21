using UnityEngine;

/// <summary>
/// Place this prefab in the level designer to trigger a dialogue conversation
/// when the camera scrolls to its Y position during gameplay.
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("Unique identifier for this prefab, used to reference it in saved level JSON.")]
    public string prefabId;

    [Tooltip("The conversation ID from the dialogue JSON database to play when triggered.")]
    public string conversationId;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.9f);
        Gizmos.DrawWireCube(transform.position, new Vector3(2f, 0.2f, 0.1f));
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 1f);
        Gizmos.DrawCube(transform.position, new Vector3(2f, 0.2f, 0.1f));

        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(
            transform.position + Vector3.right * 1.2f,
            string.IsNullOrEmpty(conversationId) ? "(no conversationId)" : conversationId
        );
    }
#endif
}
