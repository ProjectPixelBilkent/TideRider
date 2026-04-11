using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Firebase.Auth;
using UnityEngine;

public class PlayServicesManager : MonoBehaviour
{
    void Start()
    {
        PlayGamesPlatform.Activate();

        PlayGamesPlatform.Instance.Authenticate(status => {
            if (status == SignInStatus.Success)
            {
                Debug.Log("GPGS Login Success! Now Handshaking with Firebase...");
                PerformFirebaseLogin();
            }
            else
            {
                Debug.LogError("GPGS Login Failed. Check your Testers list in Play Console.");
            }
        });
    }

    private void PerformFirebaseLogin()
    {
        string authCode = PlayGamesPlatform.Instance.GetServerAuthCode();
        Credential credential = PlayGamesAuthProvider.GetCredential(authCode);

        FirebaseAuth.DefaultInstance.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task => {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("Firebase Handshake Complete. You are officially logged in!");
            }
        });
    }
}