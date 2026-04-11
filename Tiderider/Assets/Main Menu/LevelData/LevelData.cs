using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelDisplayName;
    public TextAsset levelJson;
    public int levelIndex;

    [Header("Visuals")]
    public Sprite islandSprite;
    public Color activeColor = Color.white;
}