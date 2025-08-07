using UnityEngine;
using UnityEngine.UI;
using BackEnd;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using LitJson;

public class DataRankingManager : MonoBehaviour
{


    #region Definitions

    private const int MaxVisibleRanking = 10;
    #endregion



    #region Public Variables
    #endregion



    #region Serialized Variables

    [Header("UI Components")]
    [SerializeField] private GameObject panelTrials;
    [SerializeField] private GameObject scrollview_Ranking;
    [SerializeField] private GameObject scrollView_TierRanking;
    [SerializeField] private List<GameObject> rankingRowPrefabList;

    #endregion



    #region Private Variables

    #endregion
    private bool isTierRankingMode = false;


    #region Properties

    #endregion



    #region Mono Behaviours

    #endregion



    #region Public Functions

    // Called when the ranking button is clicked
    public void OnClickRankingButton() {
        panelTrials.SetActive(true);
        RankingUIAnimator.Instance.TriggerDefaultCategorySelection("Single");
        ShowRankingView();
    }

    public void ShowTierRankingView() {
        scrollview_Ranking.SetActive(false);
        scrollView_TierRanking.SetActive(true);
    }
    public void ShowRankingView() {
        scrollview_Ranking.SetActive(true);
        scrollView_TierRanking.SetActive(false);
        ShowGameDataRanking();
    }

    // Closes the ranking panel
    public void CloseRankingPanel() {
        panelTrials.SetActive(false);
        scrollview_Ranking.SetActive(false);
        scrollView_TierRanking.SetActive(false);
    }
    public void ToggleRankingView() {
        isTierRankingMode = !isTierRankingMode;
        if (isTierRankingMode)
            ShowTierRankingView();
        else
            ShowRankingView();
    }


    #endregion



    #region Private Functions

    // Fetch and display ranking data from the server
    private void ShowGameDataRanking() {
        GetMyNickname(myNickname => {
            Backend.GameData.Get("user_data", new Where(), 300, bro => {
                if (!bro.IsSuccess()) {
                    Debug.LogError("Failed to fetch rankings: " + bro);
                    return;
                }

                List<JsonData> rows = new List<JsonData>();
                foreach (JsonData row in bro.FlattenRows()) {
                    rows.Add(row);
                }

                Debug.Log($"[Ranking] Total fetched users: {rows.Count}");

                for (int i = 0; i < rows.Count; i++) {
                    var r = rows[i];
                    string nick = r.ContainsKey("nickname") ? r["nickname"].ToString() : "Unknown";
                    string score = r.ContainsKey("max_score") ? r["max_score"].ToString() : "N/A";
                    Debug.Log($"[Ranking Raw] #{i + 1} Nickname: {nick}, Max Score: {score}");
                }

                var sorted = rows
                    .Where(row =>
                        row.ContainsKey("max_score") &&
                        row["max_score"] != null &&
                        int.TryParse(row["max_score"].ToString(), out _))
                    .OrderByDescending(row => int.Parse(row["max_score"].ToString()))
                    .ToList();

                Debug.Log($"[Ranking] Sorted user count: {sorted.Count}");

                for (int i = 0; i < sorted.Count; i++) {
                    string nick = sorted[i].ContainsKey("nickname") ? sorted[i]["nickname"].ToString() : "Unknown";
                    string score = sorted[i]["max_score"].ToString();
                    Debug.Log($"[Ranking Sorted] #{i + 1} Nickname: {nick}, Score: {score}");
                }

                // 내 순위 확인
                int myIndex = sorted.FindIndex(row =>
                    row.ContainsKey("nickname") &&
                    row["nickname"].ToString() == myNickname);

                Debug.Log($"[Ranking] My nickname: {myNickname}, Index in sorted list: {myIndex}");

                bool isInTop10 = myIndex >= 0 && myIndex < MaxVisibleRanking;

                MainThreadDispatcher.Enqueue(() => {
                    Debug.Log("[Ranking] Updating UI on main thread");

                    for (int i = 0; i < rankingRowPrefabList.Count; i++) {
                        if (rankingRowPrefabList[i] != null)
                            rankingRowPrefabList[i].SetActive(false);
                    }

                    for (int i = 0; i < Mathf.Min(MaxVisibleRanking, sorted.Count); i++) {
                        if (i >= rankingRowPrefabList.Count || rankingRowPrefabList[i] == null) {
                            Debug.LogWarning($"[Ranking] rankingRowPrefabList[{i}] is missing");
                            continue;
                        }

                        ApplyRankInfo(rankingRowPrefabList[i], sorted[i], i + 1);
                        rankingRowPrefabList[i].SetActive(true);
                    }

                    if (myIndex != -1 && myIndex >= MaxVisibleRanking && rankingRowPrefabList.Count > MaxVisibleRanking) {
                        ApplyRankInfo(rankingRowPrefabList[MaxVisibleRanking], sorted[myIndex], myIndex + 1);
                        rankingRowPrefabList[MaxVisibleRanking].SetActive(true);
                    }
                    RankingUIAnimator.Instance.OnCategorySelected("Single");
                });
            });
        });
    }





    // Applies nickname and score data to a row GameObject
    private void ApplyRankInfo(GameObject rowObj, JsonData row, int rankNumber) {
        string nickname = row.ContainsKey("nickname") ? row["nickname"].ToString() : "Unknown";
        string score = row.ContainsKey("max_score") ? row["max_score"].ToString() : "0";

        Debug.Log($"Rank Entry: {nickname} / Score: {score}");

        var rankText = rowObj.transform.Find("Text_Rank");
        var idText = rowObj.transform.Find("Text_UserID");
        var scoreText = rowObj.transform.Find("Text_UserScore");

        if (rankText == null || idText == null || scoreText == null) {
            Debug.LogError("Text components not found in prefab");
            return;
        }

        idText.GetComponent<TMP_Text>().text = nickname;
        scoreText.GetComponent<TMP_Text>().text = score;
    }

    private void GetMyNickname(System.Action<string> onNicknameReady) {
        Backend.GameData.GetMyData("user_data", new Where(), callback => {
            if (callback.IsSuccess()) {
                var row = callback.FlattenRows()[0];
                string nickname = row.ContainsKey("nickname") ? row["nickname"].ToString() : "Unknown";
                onNicknameReady?.Invoke(nickname);
            }
            else {
                Debug.LogError("Failed to fetch my nickname");
                onNicknameReady?.Invoke("Unknown");
            }
        });
    }

    #endregion
}
