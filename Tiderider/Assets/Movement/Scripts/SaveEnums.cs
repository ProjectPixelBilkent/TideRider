using System.Collections.Generic;

public enum SpawnObjectType
{
    Obstacle,
    ExternalEffect,
    Enemy,
    EndingObject,
    Coin,
    Dialogue
}

public enum TerrainType
{
    General,
    Ice,
    Misty
}

[System.Serializable]
public class SavedObjectData
{
    public string prefabId;
    public string name;
    public SpawnObjectType objectType;

    public int spriteNo;
    public TerrainType typeOfTerrain;

    public float posX;
    public float posY;
    public float posZ;

    public float rotX;
    public float rotY;
    public float rotZ;
    public float rotW;

    public float scaleX;
    public float scaleY;
    public float scaleZ;

    public string conversationId; // used for Dialogue type only
}

[System.Serializable]
public class SavedSceneData
{
    public List<SavedObjectData> objects = new List<SavedObjectData>();
}
