using UnityEngine;

[CreateAssetMenu(fileName = "GlobalUIFontConfig", menuName = "UI/Global UI Font Config")]
public class GlobalUIFontConfig : ScriptableObject
{
    public Font sourceFont;
    public int samplingPointSize = 90;
    public int atlasPadding = 9;
    public int atlasWidth = 1024;
    public int atlasHeight = 1024;
}
