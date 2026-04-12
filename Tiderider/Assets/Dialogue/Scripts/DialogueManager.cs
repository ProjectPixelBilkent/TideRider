using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private CharacterSpriteDatabase spriteDatabase;
    [SerializeField] private KeyCode continueKey = KeyCode.Space;

    private DialogueDatabase database;
    private Coroutine currentConversationRoutine;
    private string activeConversationId;

    public bool IsConversationPlaying => currentConversationRoutine != null;
    public event Action ConversationFinished;

    private void Awake()
    {
        AutoAssignDependencies();
        LoadDialogueDatabase();
    }

    private void AutoAssignDependencies()
    {
        if (dialogueController == null)
        {
            dialogueController = FindObjectOfType<DialogueController>();
        }

        if (spriteDatabase == null)
        {
            spriteDatabase = FindObjectOfType<CharacterSpriteDatabase>();
        }
    }

    private void LoadDialogueDatabase()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("dialogue");

        if (jsonFile == null)
        {
            Debug.LogError("Dialogue file not found in Assets/Resources/");
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

        if (dialogueController == null)
        {
            Debug.LogError("DialogueController reference is missing on DialogueManager.");
            return;
        }

        if (spriteDatabase == null)
        {
            Debug.LogError("CharacterSpriteDatabase reference is missing on DialogueManager.");
            return;
        }

        DialogueConversationData conversation = GetConversationById(conversationId);

        if (conversation == null)
        {
            string availableIds = string.Join(", ", database.conversations.Select(convo => convo.conversationId));
            Debug.LogError($"Conversation with ID '{conversationId}' not found. Available IDs: {availableIds}");
            return;
        }

        if (currentConversationRoutine != null)
        {
            StopCoroutine(currentConversationRoutine);
            CompleteConversation();
        }

        activeConversationId = conversationId;
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
        if (conversation.lines == null || conversation.lines.Length == 0)
        {
            Debug.LogError($"Conversation '{conversation.conversationId}' has no dialogue lines.");
            CompleteConversation();
            yield break;
        }

        foreach (var line in conversation.lines)
        {
            Sprite sprite = spriteDatabase.GetSprite(line.characterId, line.emotion);

            if (sprite == null)
            {
                Debug.LogError($"No sprite found for characterId='{line.characterId}', emotion='{line.emotion}'.");
                CompleteConversation();
                yield break;
            }

            yield return StartCoroutine(dialogueController.PlayDialogueLine(line, sprite));
            yield return StartCoroutine(WaitForContinueInput());
        }

        yield return StartCoroutine(dialogueController.HideDialogue());
        CompleteConversation();
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

    private void CompleteConversation()
    {
        currentConversationRoutine = null;
        activeConversationId = null;
        ConversationFinished?.Invoke();
    }
}
