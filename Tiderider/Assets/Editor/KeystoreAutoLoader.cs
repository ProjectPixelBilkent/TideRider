using UnityEditor;
using System.IO;
using UnityEngine;

[InitializeOnLoad]
public class KeystoreAutoLoader
{
    private static readonly string SecretsPath = "Assets/Settings/Keys/secrets.json";

    static KeystoreAutoLoader()
    {
        LoadKeystoreSettings();
    }

    [InitializeOnLoadMethod]
    private static void LoadKeystoreSettings()
    {
        if (File.Exists(SecretsPath))
        {
            try
            {
                string json = File.ReadAllText(SecretsPath);
                KeystoreData data = JsonUtility.FromJson<KeystoreData>(json);

                if (data != null)
                {
                    PlayerSettings.Android.keystoreName = data.keystorePath;
                    PlayerSettings.Android.keystorePass = data.keystorePass;
                    PlayerSettings.Android.keyaliasName = data.keyAlias;
                    PlayerSettings.Android.keyaliasPass = data.keyPass;

                    Debug.Log($"[Keystore] Successfully loaded configuration for: {data.keyAlias}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Keystore] Failed to load secrets.json: {e.Message}");
            }
        }
    }

    [System.Serializable]
    private class KeystoreData
    {
        public string keystorePath;
        public string keystorePass;
        public string keyAlias;
        public string keyPass;
    }
}