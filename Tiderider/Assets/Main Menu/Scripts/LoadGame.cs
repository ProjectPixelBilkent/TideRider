using System;
using System.Threading.Tasks;
using UnityEngine;

public class LoadGame : MonoBehaviour
{
    private async void Start()
    {
        try
        {
            await PlayServicesManager.Init();

            if (!Application.isEditor)
            {
#if UNITY_ANDROID || UNITY_IOS
                PlayServicesManager.SignIn();
#endif
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Tiderider Init Failed: {ex.Message}");
        }
    }
}