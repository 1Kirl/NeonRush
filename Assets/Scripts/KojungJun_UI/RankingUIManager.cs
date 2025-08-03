using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RankingUIManager : MonoBehaviour
{
    public static RankingUIManager Instance;

    [Header("Playing Rank UI")]
    [SerializeField] private TMP_Text[] textSlots;
    [SerializeField] private GameObject[] rankIcons; // Rank 1~8 icon


    [Header("Result Rank UI")]
    [SerializeField] private TMP_Text[] userInfoTexts;     // UserInfoList_Result 내부 Text
    [SerializeField] private TMP_Text[] bonusTexts;        // 각 결과 라인별 보너스 텍스트
    [SerializeField] private GameObject[] rewardObjs;
    [SerializeField] private GameObject[] resultRankIcons;

    private readonly Dictionary<int, int> rewardTable = new()
    {
        { 1, 50 }, { 2, 40 }, { 3, 30 },
        { 4, 15 }, { 5, 10 }, { 6, 5 },
        { 7, 3 },  { 8, 1 }
    };

    private Dictionary<ushort, int> currentScoreMap = new();
    private bool rewardGiven = false;
    private Dictionary<ushort, int> previousRankingPositions = new(); // clientId -> index


    private void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void InitRankingUI(int playerCount) {
        for (int i = 0; i < textSlots.Length; i++) {
            bool active = i < playerCount;
            textSlots[i].gameObject.SetActive(active);

            if (rankIcons != null && i < rankIcons.Length)
                rankIcons[i].SetActive(active);

            if (userInfoTexts != null && i < userInfoTexts.Length)
                userInfoTexts[i].gameObject.SetActive(active);

            if (bonusTexts != null && i < bonusTexts.Length)
                bonusTexts[i].gameObject.SetActive(false);

            if (resultRankIcons != null && i < resultRankIcons.Length)
                resultRankIcons[i].SetActive(active);

            if (rewardObjs != null && i < rewardObjs.Length)
                rewardObjs[i].gameObject.SetActive(active); 
        }
    }
    public void ShowInitialRanking(List<RankingEntry> players) {
        currentScoreMap.Clear();
        previousRankingPositions.Clear(); 
        for (int i = 0; i < players.Count && i < textSlots.Length; i++) {
            var entry = players[i];
            ushort myId = (ushort)LiteNetLibManager.Instance.inGameClientId;
            currentScoreMap[entry.ClientId] = 0;
            previousRankingPositions[entry.ClientId] = i; // rank saved.
            string text = $"{entry.Name}: 0";
            textSlots[i].text = text;
            textSlots[i].color = (entry.ClientId == myId) ? Color.green : Color.white;
            textSlots[i].gameObject.SetActive(true);
        }
    }



    // Playing Update Rank UI.
    public void UpdateRankingUI(List<RankingEntry> rankingList) {
        ushort myId = (ushort)LiteNetLibManager.Instance.inGameClientId;
        Dictionary<ushort, int> currentPositions = new(); // this frame Rank.

        for (int i = 0; i < rankingList.Count && i < textSlots.Length; i++) {
            var entry = rankingList[i];
            TMP_Text slot = textSlots[i];
            string newText = $"{entry.Name}: {entry.Score}";

            currentPositions[entry.ClientId] = i;

            bool hasChangedPosition =
                previousRankingPositions.TryGetValue(entry.ClientId, out int prevIndex) && prevIndex != i;

            bool hasChangedScore = !currentScoreMap.TryGetValue(entry.ClientId, out int oldScore) || oldScore != entry.Score;

            // UI 갱신은 항상 수행
            slot.text = newText;
            slot.color = (entry.ClientId == myId) ? Color.green : Color.white;

            // 애니메이션은 순위 변동일 때만 실행
            if (hasChangedPosition) {
                slot.rectTransform.DOKill(); // 중복 방지
                slot.rectTransform.localScale = Vector3.one; // 초기화

                // 순위 상승 -> 더 세게
                bool movedUp = prevIndex > i;

                Sequence seq = DOTween.Sequence();
                seq.Append(slot.rectTransform.DOScale(movedUp ? 1.4f : 1.2f, 0.15f).SetEase(Ease.OutQuad));
                seq.Append(slot.rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            }

            currentScoreMap[entry.ClientId] = entry.Score;
        }

        previousRankingPositions = currentPositions;
    }




    // After Game Finish.
    public void ShowResultWithBonus(List<ResultEntry> results) {
        ushort myId = (ushort)LiteNetLibManager.Instance.inGameClientId;

        results.Sort((a, b) => b.BonusScore.CompareTo(a.BonusScore));

        for (int i = 0; i < results.Count && i < userInfoTexts.Length; i++) {
            var e = results[i];
            TMP_Text infoText = userInfoTexts[i];

            string newText = $"{e.Name}: {e.BonusScore}";
            bool scoreChanged = !currentScoreMap.TryGetValue(e.ClientId, out int prevScore) || prevScore != e.BonusScore;

            // Text Change update
            if (infoText.text != newText) {
                infoText.text = newText;
                infoText.color = (e.ClientId == myId) ? Color.green : Color.white;
            }

            // If score changed Animation
            if (scoreChanged) {
                infoText.rectTransform.DOKill();
                infoText.rectTransform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1f).SetEase(Ease.OutBack);

                AnimateScoreCounter(infoText, e.Name, prevScore, e.BonusScore); 
                currentScoreMap[e.ClientId] = e.BonusScore;
            }


            // Bonus TExt (1~3)
            if (e.ArrivalRank is >= 1 and <= 3) {
                string multiplierStr = e.ArrivalRank switch {
                    1 => "2.0",
                    2 => "1.5",
                    3 => "1.25",
                    _ => "1.0"
                };

                TMP_Text bonus = bonusTexts[i];
                bonus.text = $"X{multiplierStr} Ranking Bonus";
                bonus.color = new Color(1f, 0.84f, 0f);
                bonus.fontSize = 26;

                if (!bonus.gameObject.activeSelf)
                    bonus.gameObject.SetActive(true);

                bonus.transform.DOKill();
                bonus.transform.DOPunchScale(Vector3.one * 0.25f, 0.3f, 10, 1f).SetEase(Ease.OutBack);
            }

            // Reward give (one time)
            if (!rewardGiven && e.ClientId == myId)
                GrantRankingReward(i + 1);
        }

        rewardGiven = true;
    }
    public void SetInitialResultUI(List<ResultEntry> results) {
        ushort myId = (ushort)LiteNetLibManager.Instance.inGameClientId;

        // Sort by base score (descending)
        var initialList = results
            .OrderByDescending(e => e.BaseScore)
            .ToList();

        for (int i = 0; i < initialList.Count && i < userInfoTexts.Length; i++) {
            var e = initialList[i];
            TMP_Text infoText = userInfoTexts[i];

            infoText.text = $"{e.Name}: {e.BaseScore}";
            infoText.color = (e.ClientId == myId) ? Color.green : Color.white;

            currentScoreMap[e.ClientId] = e.BaseScore;

            // 초기에는 bonus 텍스트, 애니메이션 없이
            if (bonusTexts != null && i < bonusTexts.Length)
                bonusTexts[i].gameObject.SetActive(false);
        }
    }


    private void GrantRankingReward(int myRank) {
        if (rewardTable.TryGetValue(myRank, out int coins)) {
            CurrencyManager.Instance.AddCoins(coins);
            Debug.Log($"[RankingReward] You received {coins} coins for rank {myRank}");
        }
        else {
            Debug.LogWarning($"[RankingReward] No reward defined for rank {myRank}");
        }
    }

    private void AnimateScoreCounter(TMP_Text text, string playerName, int from, int to) {
        DOVirtual.Int(from, to, 1f, value =>
        {
            text.text = $"{playerName}: {value}";
        }).SetEase(Ease.OutCubic);
    }


}
