using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private const string AlwaysReplayConversationId = "reunion_scene";

    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private CharacterSpriteDatabase spriteDatabase;
    [SerializeField] private KeyCode continueKey = KeyCode.Space;

    private DialogueDatabase database;
    private Coroutine currentConversationRoutine;
    private Coroutine currentLineRoutine;
    private string activeConversationId;
    private bool markActiveConversationAsCompleted = true;

    public bool IsConversationPlaying => currentConversationRoutine != null;
    public event Action ConversationFinished;

    public void ConfigureDependencies(DialogueController controller, CharacterSpriteDatabase databaseRef)
    {
        dialogueController = controller;
        spriteDatabase = databaseRef;
    }

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
        PlayConversation(conversationId, true);
    }

    public void PlayConversation(string conversationId, bool markCompleted)
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

        bool shouldSkipBecauseCompleted = !string.Equals(conversationId, AlwaysReplayConversationId, StringComparison.OrdinalIgnoreCase)
            && DataManager.IsConversationCompleted(conversationId);

        if (shouldSkipBecauseCompleted)
        {
            Debug.Log($"Conversation '{conversationId}' has already been played. Skipping.");
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
            if (currentLineRoutine != null)
            {
                StopCoroutine(currentLineRoutine);
                currentLineRoutine = null;
            }
            CompleteConversation();
        }

        activeConversationId = conversationId;
        dialogueController.ClearSkipConversationRequest();
        markActiveConversationAsCompleted = markCompleted;
        currentConversationRoutine = StartCoroutine(PlayConversationRoutine(conversation));
    }

    public void SkipConversation()
    {
        if (currentConversationRoutine == null || dialogueController == null)
        {
            return;
        }

        dialogueController.RequestSkipConversation();
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

        dialogueController.ClearSkipConversationRequest();

        foreach (var line in conversation.lines)
        {
            Sprite sprite = spriteDatabase.GetSprite(line.characterId, line.emotion);

            if (sprite == null)
            {
                Debug.LogError($"No sprite found for characterId='{line.characterId}', emotion='{line.emotion}'.");
                CompleteConversation();
                yield break;
            }

            bool lineComplete = false;
            currentLineRoutine = StartCoroutine(PlayLineAndSignal(line, sprite, () => lineComplete = true));

            while (!lineComplete)
            {
                if (dialogueController.ConsumeSkipConversationRequest())
                {
                    if (currentLineRoutine != null)
                    {
                        StopCoroutine(currentLineRoutine);
                        currentLineRoutine = null;
                    }

                    yield return StartCoroutine(dialogueController.HideDialogue());
                    CompleteConversation();
                    yield break;
                }

                if (IsInputPressed() && dialogueController.IsTyping)
                {
                    dialogueController.SkipTyping();
                }
                yield return null;
            }

            currentLineRoutine = null;
            yield return null;

            bool pressed = false;
            while (!pressed)
            {
                if (dialogueController.ConsumeSkipConversationRequest())
                {
                    yield return StartCoroutine(dialogueController.HideDialogue());
                    CompleteConversation();
                    yield break;
                }

                if (IsInputPressed())
                {
                    pressed = true;
                }

                yield return null;
            }
        }

        yield return StartCoroutine(dialogueController.HideDialogue());
        CompleteConversation();
    }

    private IEnumerator PlayLineAndSignal(DialogueLineData line, Sprite sprite, System.Action onComplete)
    {
        yield return StartCoroutine(dialogueController.PlayDialogueLine(line, sprite));
        onComplete?.Invoke();
    }

    private bool IsInputPressed()
    {
        return Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
    }

    private IEnumerator WaitForContinueInput()
    {
        yield return null;

        bool pressed = false;
        while (!pressed)
        {
            if (IsInputPressed())
            {
                pressed = true;
            }

            yield return null;
        }
    }

    private void CompleteConversation()
    {
        if (markActiveConversationAsCompleted && !string.IsNullOrEmpty(activeConversationId))
        {
            DataManager.MarkConversationCompleted(activeConversationId);
        }

        currentLineRoutine = null;
        currentConversationRoutine = null;
        activeConversationId = null;
        markActiveConversationAsCompleted = true;
        ConversationFinished?.Invoke();
    }
}
