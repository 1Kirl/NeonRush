using BackEnd;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject achievementPanel;

    [Header("Achievement UI")]
    public AchievementItemUI maxScoreUI;
    public AchievementItemUI flipCountUI;
    public AchievementItemUI coinSpentUI;
    public AchievementItemUI winCountUI;

    [Header("Target Values")]
    public int maxScoreTarget = 300;
    public int flipTarget = 50;
    public int coinTarget = 1000;
    public int winTarget = 10;

    [Header("Unlockable Items")]
    public List<GameObject> unlockableItems; // isLocked bool ���� ��ũ��Ʈ �����ϰų� �׳� ������Ʈ Ȱ��ȭ ���η� �Ǵ�

    public static AchievementManager Instance;

    public void ClosePanel() {
        achievementPanel.SetActive(false);  
    }
    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        RefreshProgress();
    }
    public void LoadAndApplyAchievements(int maxScore, int flipCount, int coinSpent, int winCount) {
        MainThreadDispatcher.Enqueue(() => {
            maxScoreUI.Setup(maxScoreTarget);
            maxScoreUI.UpdateProgress(maxScore);

            flipCountUI.Setup(flipTarget);
            flipCountUI.UpdateProgress(flipCount);

            coinSpentUI.Setup(coinTarget);
            coinSpentUI.UpdateProgress(coinSpent);

            winCountUI.Setup(winTarget);
            winCountUI.UpdateProgress(winCount);

            if (maxScore >= maxScoreTarget) UnlockItem(0);
            if (flipCount >= flipTarget) UnlockItem(1);
            if (coinSpent >= coinTarget) UnlockItem(2);
            if (winCount >= winTarget) UnlockItem(3);
        });
    }


    public void UnlockItem(int index) {
        string[] messages = {
        "You hit 300 points!",
        "Flipping master unlocked!",
        "Big spender vibes!",
        "Victory streak legend!"
    };

        if (index >= unlockableItems.Count) return;

        var item = unlockableItems[index];
        if (item.TryGetComponent(out ShopItemButton shopButton)) {
            shopButton.Unlock();
        }
        else {
            item.SetActive(true);
        }

        switch (index) {
            case 0: maxScoreUI.UnlockReward(); break;
            case 1: flipCountUI.UnlockReward(); break;
            case 2: coinSpentUI.UnlockReward(); break;
            case 3: winCountUI.UnlockReward(); break;
        }
    }



    // ���� �߰����� ������ �� �ְ� �޼��� ����
    public void RefreshProgress() {
        // 서버에서 업적 기준 데이터 직접 조회 후 업적 갱신
        Where where = new Where();
        where.Equal("owner_inDate", BackEnd.Backend.UserInDate);

        BackEnd.Backend.GameData.Get("user_data", where, callback => {
            if (callback.IsSuccess() && callback.FlattenRows().Count > 0) {
                var row = callback.FlattenRows()[0];

                int maxScore = row.ContainsKey("max_score") ? int.Parse(row["max_score"].ToString()) : 0;
                int flipCount = row.ContainsKey("flip_count") ? int.Parse(row["flip_count"].ToString()) : 0;
                int coinSpent = row.ContainsKey("coin_spent") ? int.Parse(row["coin_spent"].ToString()) : 0;
                int winCount = row.ContainsKey("win_count") ? int.Parse(row["win_count"].ToString()) : 0;

                MainThreadDispatcher.Enqueue(() => {
                    LoadAndApplyAchievements(maxScore, flipCount, coinSpent, winCount);
                });
            }
            else {
                Debug.LogWarning("[AchievementManager] 유저 업적 데이터 조회 실패 또는 없음");
            }
        });
    }


    public void OpenAchievementPanel() {
        if (achievementPanel != null) {
            achievementPanel.SetActive(true);
            RefreshProgress(); // �ֽ� ������ �ݿ�
        }
    }

    public void CloseAchievementPanel() {
        if (achievementPanel != null) {
            achievementPanel.SetActive(false);
        }
    }
}
