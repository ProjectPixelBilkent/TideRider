//using UnityEngine;

//public class DialogueTester : MonoBehaviour
//{
//    [SerializeField] private DialogueManager dialogueManager;
//    [SerializeField] private string conversationId = "scene_0";

//    private void Start()
//    {
//        if (dialogueManager == null)
//        {
//            dialogueManager = FindObjectOfType<DialogueManager>();
//        }

//        if (dialogueManager == null)
//        {
//            Debug.LogError("DialogueTester could not find a DialogueManager in the scene.");
//            return;
//        }

//        Debug.Log("DialogueTester.Start ran");
//        dialogueManager.PlayConversation(conversationId);
//    }
//}
