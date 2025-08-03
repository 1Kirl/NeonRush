#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using BackEnd;

public class DummyUserSeeder : MonoBehaviour
{
    [ContextMenu("Create Dummy Users")]
    public void CreateDummyUsers() {
#if UNITY_EDITOR
        string[] dummyNicknames = { "Alpha", "Bravo", "Charlie", "Delta" };
        int[] dummyScores = { 10, 50, 30, 20 };

        for (int i = 0; i < dummyNicknames.Length; i++) {
            string id = System.Guid.NewGuid().ToString();
            string pw = System.Guid.NewGuid().ToString();

            int score = dummyScores[i];
            string nickname = dummyNicknames[i];

            Backend.BMember.CustomSignUp(id, pw, signUpCallback => {
                if (signUpCallback.IsSuccess()) {
                    Backend.BMember.CustomLogin(id, pw, loginCallback => {
                        if (loginCallback.IsSuccess()) {
                            Param param = new Param();
                            param.Add("nickname", nickname);
                            param.Add("max_score", score);

                            Backend.GameData.Insert("user_data", param, insertCallback => {
                                if (insertCallback.IsSuccess()) {
                                    Debug.Log("Dummy user inserted: " + nickname);
                                }
                                else {
                                    Debug.LogError("Insert failed: " + insertCallback);
                                }
                            });
                        }
                    });
                }
            });
        }
#else
        Debug.LogWarning("This is only available in the Unity Editor.");
#endif
    }
}
