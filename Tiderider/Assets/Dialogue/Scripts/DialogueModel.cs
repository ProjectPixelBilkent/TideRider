using System;
using UnityEngine;

[Serializable]
public class DialogueLineData
{
    public string text;
    public string characterId;
    public string emotion;
    public DialogueSide enterSide;
}

[Serializable]
public class DialogueConversationData
{
    public string conversationId;
    public DialogueLineData[] lines;
}

[Serializable]
public class DialogueDatabase
{
    public DialogueConversationData[] conversations;
}

public enum DialogueSide
{
    Left = 0,
    Right = 1,
    Middle = 2
}