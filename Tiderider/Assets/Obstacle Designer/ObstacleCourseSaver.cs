using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SaveCurrentObstacleCourse : EditorWindow
{
    private const string SavePath = "Assets/Resources/ObstacleCourseData.json";

    [MenuItem("Tools/Save Current Obstacle Course")]
    public static void ShowWindow()
    {
        GetWindow(typeof(SaveCurrentObstacleCourse), false, "Save Current Obstacle Course");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("JSON Save Path:", SavePath);

        if (!GUILayout.Button("Save Current Obstacle Course to JSON"))
        {
            return;
        }


        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] allObjects = activeScene.GetRootGameObjects();

        float xLeft = 0;
        float xRight = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "Left Border")
            {
                xLeft = obj.transform.position.x;
            }
            if (obj.name == "Right Border")
            {
                xRight = obj.transform.position.x;
            }
        }

        List<ObstacleInformation> obstacleList = new List<ObstacleInformation>();

        Obstacle[] obstacles = Resources.FindObjectsOfTypeAll<Obstacle>()
            .Where(o => o.gameObject.scene == activeScene)
            .ToArray();

        foreach (Obstacle obstacle in obstacles)
        {
            GameObject obj = obstacle.gameObject;

            ObstacleType type;
            if (System.Enum.TryParse(obj.name, true, out ObstacleType parsedType))
            {
                type = parsedType;
            }
            else
            {
                type = ObstacleType.Normal;
            }

            ObstacleInformation info = new ObstacleInformation
            {
                type = type,
                Position = obj.transform.position,
                Scale = obj.transform.localScale,
                WarningPlacement = Vector3.zero
            };

            obstacleList.Add(info);
        }

        var courseData = new CourseData
        {
            leftX = xLeft,
            rightX = xRight,
            obstacles = obstacleList
        };

        string json = JsonUtility.ToJson(courseData, true);

        string fullPath = Path.Combine(Application.dataPath.Replace("Assets", ""), SavePath);

        try
        {
            File.WriteAllText(fullPath, json);
            Debug.Log($"Successfully saved obstacle course data to: **{SavePath}**");
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save JSON file at {fullPath}. Error: {e.Message}");
        }
    }

    [System.Serializable]
    public class CourseData
    {
        public float leftX;
        public float rightX;
        public List<ObstacleInformation> obstacles = new List<ObstacleInformation>();
    }

    [System.Serializable]
    public struct ObstacleInformation
    {
        public ObstacleType type;

        public SerializableVector3 position;
        public SerializableVector3 scale;
        public SerializableVector3 warningPlacement;

        public Vector3 Position
        {
            set => position = new SerializableVector3(value);
            get => position.ToVector3();
        }
        public Vector3 Scale
        {
            set => scale = new SerializableVector3(value);
            get => scale.ToVector3();
        }
        public Vector3 WarningPlacement
        {
            set => warningPlacement = new SerializableVector3(value);
            get => warningPlacement.ToVector3();
        }
    }

    [System.Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}

public enum ObstacleType
{
    Normal, Big, Huge, Surprise, DownwardStream, UpwardStream
}
