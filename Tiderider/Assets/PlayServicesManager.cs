#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
using UnityEngine;
using System.Threading.Tasks;

public static class PlayServicesManager
{
    public static async Task Init()
    {
        // PlayGamesPlatform.Activate();
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
}
