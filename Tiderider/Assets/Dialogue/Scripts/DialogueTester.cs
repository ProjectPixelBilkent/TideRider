using UnityEngine;

public class DialogueTester : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private string conversationId = "intro_scene";

    private void Start()
    {
        Debug.Log("DialogueTester.Start ran");
        dialogueManager.PlayConversation(conversationId);
    }
}