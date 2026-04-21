using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Firebase.Auth;
using UnityEngine;
using System.Threading.Tasks;

public static class PlayServicesManager
{
    public static async Task Init()
    {
        // PlayGamesPlatform.Activate();
        var dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == Firebase.DependencyStatus.Available)
        {
            var auth = FirebaseAuth.DefaultInstance;
        }
        else
        {
            Debug.LogError($"Firebase Error: {dependencyStatus}");
        }
    }

    public static void SignIn()
    {
        // if (!PlayGamesPlatform.Instance.IsAuthenticated())
        // {
        //     PlayGamesPlatform.Instance.Authenticate(status =>
        //     {
        //         Debug.Log($"Play Games Auth Status: {status}");
        //         if (status == SignInStatus.Success)
        //         {
        //             PlayGamesPlatform.Instance.RequestServerSideAccess(true, code => LinkWithFirebase(code));
        //         }
        //     });
        // }
    }

    private static void LinkWithFirebase(string authCode)
    {
        Credential credential = PlayGamesAuthProvider.GetCredential(authCode);
        FirebaseAuth.DefaultInstance.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("Firebase Link Success");
            }
            else
            {
                Debug.LogError($"Firebase Link Failed: {task.Exception}");
            }
        });
    }
}
