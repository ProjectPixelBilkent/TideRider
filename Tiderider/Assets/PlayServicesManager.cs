using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Firebase.Auth;
using UnityEngine;

public class PlayServicesManager : MonoBehaviour
{
    void Start()
    {
        // 1. Activate the platform. 
        // Configuration is now pulled from the 'Android Setup' XML you pasted in the Editor.
        PlayGamesPlatform.Activate();

        // 2. Trigger Login
        SignIn();
    }

    public void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status == SignInStatus.Success)
            {
                Debug.Log("[GPGS] Login Successful!");

                // 3. This is the new way to get the code for Firebase
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    Debug.Log($"[GPGS] Received Auth Code: {code}");
                    LinkWithFirebase(code);
                });
            }
            else
            {
                Debug.LogError($"[GPGS] Login Failed. Status: {status}");
            }
        });
    }

    private void LinkWithFirebase(string authCode)
    {
        // This part remains the same
        Credential credential = PlayGamesAuthProvider.GetCredential(authCode);
        FirebaseAuth.DefaultInstance.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("[Firebase] Successfully Handshaked with Google Play Services!");
            }
            else
            {
                Debug.LogError("[Firebase] Handshake Failed. Verify your Web Client ID in the Unity GPGS Setup window.");
            }
        });
    }
}