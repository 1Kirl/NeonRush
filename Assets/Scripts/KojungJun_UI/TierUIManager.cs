using UnityEngine;
using UnityEngine.UI;
using BackEnd;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using LitJson;

public class TierUIManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject scrollview;
    [SerializeField] private List<GameObject> rankingRowPrefabList;
    [SerializeField] private List<Sprite> tierSprites; // Order: Bronze~Challenger(0~5)
    [SerializeField] private RankingUIAnimator rankingUIAnimator;
    private const int MaxVisibleRanking = 10;

    public void RefreshTierRank() {
        ShowTierRanking();
    }

    private void ShowTierRanking() {
        GetMyNickname(myNickname => {
            Backend.GameData.Get("user_data", new Where(), bro => {
                if (!bro.IsSuccess()) {
                    Debug.LogError("Failed to fetch tier rankings: " + bro);
                    return;
                }

                List<JsonData> rows = new List<JsonData>();

                foreach (JsonData row in bro.FlattenRows()) {
                    rows.Add(row);
                }
                // Sorting by tierScore.
                var sorted = rows
                    .Where(row => row.ContainsKey("tierScore") &&
                                  row["tierScore"] != null &&
                                  int.TryParse(row["tierScore"].ToString(), out _))
                    .OrderByDescending(row => int.Parse(row["tierScore"].ToString()))
                    .ToList();

                int myIndex = sorted.FindIndex(row =>
                    row.ContainsKey("nickname") &&
                    row["nickname"].ToString() == myNickname);

                MainThreadDispatcher.Enqueue(() => {
                    for (int i = 0; i < rankingRowPrefabList.Count; i++) {
                        if (rankingRowPrefabList[i] != null)
                            rankingRowPrefabList[i].SetActive(false);
                    }

                    for (int i = 0; i < Mathf.Min(MaxVisibleRanking, sorted.Count); i++) {
                        if (i >= rankingRowPrefabList.Count || rankingRowPrefabList[i] == null)
                            continue;

                        ApplyTierRankInfo(rankingRowPrefabList[i], sorted[i], i + 1);
                        rankingRowPrefabList[i].SetActive(true);
                    }

                    // Over 10 my rank.
                    if (myIndex != -1 && myIndex >= MaxVisibleRanking && rankingRowPrefabList.Count > MaxVisibleRanking) {
                        ApplyTierRankInfo(rankingRowPrefabList[MaxVisibleRanking], sorted[myIndex], myIndex + 1);
                        rankingRowPrefabList[MaxVisibleRanking].SetActive(true);
                    }
                    RankingUIAnimator.Instance.OnCategorySelected("Multi");

                });
            });
        });
    }

    private void ApplyTierRankInfo(GameObject rowObj, JsonData row, int rankNumber) {
        string nickname = row.ContainsKey("nickname") ? row["nickname"].ToString() : "Unknown";
        int tierScore = row.ContainsKey("tierScore") ? int.Parse(row["tierScore"].ToString()) : 0;

        // Tier Index (TierManager)
        int tierIndex = TierManager.GetTierIndexByScore(tierScore);
        int tierType = tierIndex / 4; // 0:Bronze, 1. Silver, 2: Gold, 3:Platinum, 4: Diamond, 5:Challenger

        // UI Binding
        var idText = rowObj.transform.Find("Text_UserID");
        var tierScoreText = rowObj.transform.Find("Text_UserTierScore");
        var tierImageObj = rowObj.transform.Find("Tier_UserTier");

        if (idText != null)
            idText.GetComponent<TMP_Text>().text = nickname;
        if (tierScoreText != null)
            tierScoreText.GetComponent<TMP_Text>().text = tierScore.ToString();

        // Tier Image (0~5)
        if (tierImageObj != null && tierSprites.Count > tierType)
            tierImageObj.GetComponent<Image>().sprite = tierSprites[tierType];
    }

    private void GetMyNickname(System.Action<string> onNicknameReady) {
        Backend.GameData.GetMyData("user_data", new Where(), callback => {
            if (callback.IsSuccess()) {
                var row = callback.FlattenRows()[0];
                string nickname = row.ContainsKey("nickname") ? row["nickname"].ToString() : "";
                onNicknameReady?.Invoke(nickname);
            }
            else {
                Debug.LogError("Failed to fetch my nickname");
                onNicknameReady?.Invoke("");
            }
        });
    }
}
