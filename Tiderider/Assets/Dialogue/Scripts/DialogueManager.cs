using System.Collections;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private CharacterSpriteDatabase spriteDatabase;
    [SerializeField] private KeyCode continueKey = KeyCode.Space;

    private DialogueDatabase database;
    private Coroutine currentConversationRoutine;

    private void Awake()
    {
        LoadDialogueDatabase();
    }

    private void LoadDialogueDatabase()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("dialogue");

        if (jsonFile == null)
        {
            Debug.LogError("dialogues.json not found in Assets/Resources/");
            return;
        }

        database = JsonUtility.FromJson<DialogueDatabase>(jsonFile.text);

        if (database == null || database.conversations == null)
        {
            Debug.LogError("Failed to parse dialogue database.");
        }
    }

    public void PlayConversation(string conversationId)
    {
        if (database == null)
        {
            Debug.LogError("Dialogue database is not loaded.");
            return;
        }

        DialogueConversationData conversation = GetConversationById(conversationId);

        if (conversation == null)
        {
            Debug.LogError($"Conversation with ID '{conversationId}' not found.");
            return;
        }

        if (currentConversationRoutine != null)
        {
            StopCoroutine(currentConversationRoutine);
        }

        currentConversationRoutine = StartCoroutine(PlayConversationRoutine(conversation));
    }

    private DialogueConversationData GetConversationById(string conversationId)
    {
        foreach (var conversation in database.conversations)
        {
            if (conversation.conversationId == conversationId)
            {
                return conversation;
            }
        }

        return null;
    }

    private IEnumerator PlayConversationRoutine(DialogueConversationData conversation)
    {
        foreach (var line in conversation.lines)
        {
            Sprite sprite = spriteDatabase.GetSprite(line.characterId, line.emotion);
            yield return StartCoroutine(dialogueController.PlayDialogueLine(line, sprite));
            yield return StartCoroutine(WaitForContinueInput());
        }
        yield return StartCoroutine(dialogueController.HideDialogue());
        currentConversationRoutine = null;
    }

    private IEnumerator WaitForContinueInput()
    {
        yield return null;
        bool pressed = false;
        while (!pressed)
        {
            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
            {
                pressed = true;
            }
            yield return null;
        }
    }

}