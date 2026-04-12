using System;
using UnityEngine;

[Serializable]
public class CharacterEmotionSprite
{
    public string characterId;
    public string emotion;
    public Sprite sprite;
}

public class CharacterSpriteDatabase : MonoBehaviour
{
    [SerializeField] private CharacterEmotionSprite[] spriteEntries;

    public Sprite GetSprite(string characterId, string emotion)
    {
        print("---------");
        print(characterId + ", " +  emotion);
        foreach (var entry in spriteEntries)
        {
            print(entry.characterId + ", " + entry.emotion);
            if (entry.characterId == characterId && entry.emotion == emotion)
            {
                return entry.sprite;
            }
        }

        Debug.LogWarning($"No sprite found for characterId='{characterId}', emotion='{emotion}'.");
        return null;
    }
}