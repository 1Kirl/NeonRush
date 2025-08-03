using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BackEnd;
using System.Text.RegularExpressions;
using LitJson;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections.Generic;

public class BackendManager : MonoBehaviour
{
    private const string GuestIdKey = "guest_id";
    private const string GuestPwKey = "guest_pw";
    private bool _isSubmittingNickname = false;
    private static readonly string[] AchievementUnlockKeys = {
    "achievement_unlocked_0",
    "achievement_unlocked_1",
    "achievement_unlocked_2",
    "achievement_unlocked_3"
};

    [SerializeField] private string mainScene = "";
    [Header("Debug Options")]
    [SerializeField] private bool isDebugMode = true;
    [SerializeField] private bool resetBool = false;

    [Header("UI Components")]
    [SerializeField] private Button guestLoginButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button startGameButton;
    [SerializeField] private GameObject textGameStart;
    [SerializeField] private CarBuildManager carBuildManager;

    [Header("Nickname UI")]
    [SerializeField] private GameObject nicknamePanel;
    [SerializeField] private TMP_InputField inputNickname;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button buttonMakeNickname;
    [SerializeField] private Button buttonConfirmNickname;

    private void Start() {
        var result = Backend.Initialize();
        if (!result.IsSuccess()) {
            Debug.LogError("Backend initialization failed: " + result);
            return;
        }

        Debug.Log("Backend initialized successfully");

        startGameButton.gameObject.SetActive(false);
        textGameStart.SetActive(false);
        nicknamePanel.SetActive(false);

        buttonConfirmNickname.onClick.AddListener(OnClickConfirmNickname);
        buttonMakeNickname.onClick.AddListener(ShowNicknamePanel);
        startGameButton.onClick.AddListener(OnStartGameClicked);

        if (resetBool)
            ResetGuestLoginData(); // 테스트용

        string guestId = PlayerPrefs.GetString(GuestIdKey, "");
        string guestPw = PlayerPrefs.GetString(GuestPwKey, "");

        if (!string.IsNullOrEmpty(guestId) && !string.IsNullOrEmpty(guestPw))
            CustomLogin(guestId, guestPw);
        else {
            guestLoginButton.gameObject.SetActive(true);
            guestLoginButton.onClick.RemoveAllListeners();
            guestLoginButton.onClick.AddListener(OnGuestLoginClicked);
        }
    }

    private void OnGuestLoginClicked() {
        string guestId = System.Guid.NewGuid().ToString();
        string guestPw = System.Guid.NewGuid().ToString();

        PlayerPrefs.SetString(GuestIdKey, guestId);
        PlayerPrefs.SetString(GuestPwKey, guestPw);

        Backend.BMember.CustomSignUp(guestId, guestPw, callback => {
            if (callback.IsSuccess()) {
                Debug.Log("Guest sign-up successful");
                CustomLogin(guestId, guestPw);
            }
            else {
                Debug.LogError("Guest sign-up failed: " + callback);
            }
        });
    }

    private void CustomLogin(string id, string pw) {
        Backend.BMember.CustomLogin(id, pw, callback => {
            if (!callback.IsSuccess()) {
                Debug.LogError("Guest login failed: " + callback);
                return;
            }

            Debug.Log("Guest login successful");

            Where where = new Where();
            where.Equal("owner_inDate", Backend.UserInDate);

            Backend.GameData.Get("user_data", where, getCallback => {
                MainThreadDispatcher.Enqueue(() => {
                    guestLoginButton.gameObject.SetActive(false);

                    if (getCallback.IsSuccess() && getCallback.FlattenRows().Count > 0) {
                        var row = getCallback.FlattenRows()[0];
                        if (row.ContainsKey("nickname") && !string.IsNullOrEmpty(row["nickname"].ToString())) {
                            PlayerPrefs.SetString("nickname", row["nickname"].ToString());
                            PlayerPrefs.Save();

                            buttonMakeNickname.gameObject.SetActive(false);
                            textGameStart.SetActive(true);
                            startGameButton.gameObject.SetActive(true);
                        }
                        else {
                            buttonMakeNickname.gameObject.SetActive(true);
                            textGameStart.SetActive(false);
                            startGameButton.gameObject.SetActive(false);
                        }
                    }
                    else {
                        buttonMakeNickname.gameObject.SetActive(true);
                        textGameStart.SetActive(false);
                        startGameButton.gameObject.SetActive(false);
                    }

                    SyncAllStatsFromServer();
                });
            });
        });
    }

    private void ShowNicknamePanel() {
        MainThreadDispatcher.Enqueue(() => {
            nicknamePanel.SetActive(true);
        });
    }

    private void OnClickConfirmNickname() {
        if (_isSubmittingNickname) return;
        _isSubmittingNickname = true;

        string nickname = inputNickname.text.Trim();

        if (string.IsNullOrEmpty(nickname) || nickname.Length > 8 || !Regex.IsMatch(nickname, "^[a-zA-Z0-9]{1,8}$")) {
            feedbackText.text = "Nickname must be 1~8 characters using only letters and digits.";
            _isSubmittingNickname = false;
            return;
        }

        Where where = new Where();
        where.Equal("nickname", nickname);

        Backend.GameData.Get("user_data", where, callback => {
            if (!callback.IsSuccess()) {
                MainThreadDispatcher.Enqueue(() => {
                    feedbackText.text = "Nickname check failed.";
                    _isSubmittingNickname = false;
                });
                return;
            }

            if (callback.Rows().Count > 0) {
                MainThreadDispatcher.Enqueue(() => {
                    feedbackText.text = "This nickname is already in use.";
                    _isSubmittingNickname = false;
                });
                return;
            }

            Param param = new Param();
            param.Add("nickname", nickname);

            Where userWhere = new Where();
            userWhere.Equal("owner_inDate", Backend.UserInDate);

            Backend.GameData.Get("user_data", userWhere, userCallback => {
                if (userCallback.IsSuccess() && userCallback.Rows().Count > 0) {
                    Backend.GameData.Update("user_data", userWhere, param, updateCallback => {
                        MainThreadDispatcher.Enqueue(() => {
                            _isSubmittingNickname = false;
                            if (updateCallback.IsSuccess()) {
                                feedbackText.text = "Nickname update success";
                                nicknamePanel.SetActive(false);
                                buttonMakeNickname.gameObject.SetActive(false);
                                textGameStart.SetActive(true);
                                startGameButton.gameObject.SetActive(true);
                            }
                            else {
                                feedbackText.text = "Failed to update the nickname.";
                            }
                        });
                    });
                }
                else {
                    // 최초 회원가입: 기본 데이터 세팅
                    Debug.Log("[BackendManager] else!!!");
                    param.Add("max_score", 0);
                    param.Add("flip_count", 0);
                    param.Add("coin_spent", 0);
                    param.Add("win_count", 0);

                    param.Add("coins",8000);
                    param.Add("unlockedCarIds", new List<int> { 0 });
                    param.Add("equippedCarId", 0);
                    param.Add("equippedDieEffectIds", 0);
                    param.Add("unlockedDieEffectIds", new List<int> { 0 });
                    param.Add("equippedTrail", 0);
                    param.Add("unlockedTrail", new List<int> { 0 });
                    param.Add("tierScore", 0);
                    param.Add("bestTier", 0);

                    Backend.GameData.Insert("user_data", param, insertCallback => {
                        MainThreadDispatcher.Enqueue(() => {
                            _isSubmittingNickname = false;
                            if (insertCallback.IsSuccess()) {
                                feedbackText.text = "Nickname registered successfully.";
                                nicknamePanel.SetActive(false);
                                buttonMakeNickname.gameObject.SetActive(false);
                                textGameStart.SetActive(true);
                                startGameButton.gameObject.SetActive(true);
                                PlayerPrefs.SetString("nickname", nickname);
                                PlayerPrefs.Save();
                            }
                            else {
                                feedbackText.text = "Failed to register nickname.";
                            }
                        });
                    });
                }
            });
        });
    }

    private void OnStartGameClicked() {
        float fadeDuration = 1.5f;
        backgroundImage.DOFade(0.6f, fadeDuration).OnComplete(() => {
            SceneManager.LoadScene(mainScene);
        });
    }

    [ContextMenu("Reset Guest Login (Test Only)")]
    private void ResetGuestLoginData() {
        if (IsDebugEnvironment()) {
            Backend.BMember.WithdrawAccount(callback => {
                if (callback.IsSuccess())
                    Debug.Log("Server account withdrawal successful");
                else
                    Debug.LogWarning("Server account withdrawal failed: " + callback);
            });
        }

        PlayerPrefs.DeleteAll(); // 더 간단하게 리셋

        // 업적 해금 상태 초기화 (중복 보장용)
        foreach (var key in AchievementUnlockKeys)
            PlayerPrefs.DeleteKey(key);

        PlayerPrefs.Save();

        guestLoginButton.gameObject.SetActive(true);
        startGameButton.gameObject.SetActive(false);
        buttonMakeNickname.gameObject.SetActive(false);
        nicknamePanel.SetActive(false);
        textGameStart.SetActive(false);
    }


    private bool IsDebugEnvironment() {
#if UNITY_EDITOR
        return isDebugMode;
#else
        return isDebugMode && Debug.isDebugBuild;
#endif
    }

    private void SyncAllStatsFromServer() {
        Where where = new Where();
        where.Equal("owner_inDate", Backend.UserInDate);

        Backend.GameData.Get("user_data", where, callback => {
            if (callback.IsSuccess() && callback.FlattenRows().Count > 0) {
                var row = callback.FlattenRows()[0];

                MainThreadDispatcher.Enqueue(() => {
                    int maxScore = row.ContainsKey("max_score") ? int.Parse(row["max_score"].ToString()) : 0;
                    int flipCount = row.ContainsKey("flip_count") ? int.Parse(row["flip_count"].ToString()) : 0;
                    int coinSpent = row.ContainsKey("coin_spent") ? int.Parse(row["coin_spent"].ToString()) : 0;
                    int winCount = row.ContainsKey("win_count") ? int.Parse(row["win_count"].ToString()) : 0;
                    int tierScore = row.ContainsKey("tierScore") ? int.Parse(row["tierScore"].ToString()) : 0;
                    int bestTier = row.ContainsKey("bestTier") ? int.Parse(row["bestTier"].ToString()) : 0;

                    // PlayerPrefs도 같이 저장 (호환성 및 임시 보존용)
                    PlayerPrefs.SetInt("max_score", maxScore);
                    PlayerPrefs.SetInt("flip_count", flipCount);
                    PlayerPrefs.SetInt("coin_spent", coinSpent);
                    PlayerPrefs.SetInt("win_count", winCount);
                    PlayerPrefs.SetInt("tierScore", tierScore);
                    PlayerPrefs.SetInt("bestTier", bestTier);
                    PlayerPrefs.Save();

                    Debug.Log("[Backend] 업적 관련 데이터 PlayerPrefs 저장 완료");
                });
            }
            else {
                Debug.LogWarning("유저 데이터 가져오기 실패 또는 데이터 없음");
            }
        });
    }


}
