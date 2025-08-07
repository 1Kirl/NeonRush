using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using BackEnd;

public class TierScoreResultUI : MonoBehaviour
{
    [Header("Ƽ��/���� UI")]
    public Image tierImage;
    public Sprite[] tierSprites; // TierManager.TierTable.Count��ŭ
    public TMP_Text scoreText;

    [Header("���� �Ķ����")]
    public float scoreCountDuration = 1.2f;
    public float punchScale = 1.2f;
    public float punchDuration = 0.35f;

    [Header("�г�")]
    public GameObject rootPanel;

    Coroutine scoreRoutine;

    /// <summary>
    /// Ƽ��/���� ���� (���� ��ȭ��Ƽ�� ���� �ڵ�)
    /// </summary>
    /// 
    public static TierScoreResultUI Instance { get; private set; }
    void Awake() {
        Instance = this;
    }

    public void PlayTierScoreResult(int prevScore, int finalScore) {
        rootPanel.SetActive(true);

        if (scoreRoutine != null)
            StopCoroutine(scoreRoutine);
        scoreRoutine = StartCoroutine(AnimateScoreAndTier(prevScore, finalScore));
    }
    public void AnimateAndUpdateTierScore(int myRank) {
        // 1. ���� Ƽ�� ���� �ҷ����� (���� ĳ��/PlayerPrefs����, ���� ����ȭ ��)
        int prevScore = PlayerPrefs.GetInt("tierScore", 0);

        // 2. ��ȭ�� ���
        int delta = GetScoreDeltaByRank(myRank);
        int finalScore = Mathf.Max(0, prevScore + delta); // ���� ����

        // 3. �ִϸ��̼� ����
        PlayTierScoreResult(prevScore, finalScore);

        // 4. ���� ĳ�õ� �̸� ������Ʈ (���� ��� �� UI �żӼ� ��)
        PlayerPrefs.SetInt("tierScore", finalScore);
        PlayerPrefs.Save();

        // 5. ����(DB)�� ���� ���� �ݿ� (�񵿱�)
        UpdateTierScoreToBackend(finalScore);
    }

    /// <summary>
    /// ����(DB)�� tierScore�� ������Ʈ�ϴ� �Լ� ���� (�ڳ� SDK ����)
    /// </summary>
    private void UpdateTierScoreToBackend(int newScore) {
        Param param = new Param();
        param.Add("tierScore", newScore);

        Where where = new Where();
        where.Equal("owner_inDate", BackEnd.Backend.UserInDate);

        BackEnd.Backend.GameData.Update("user_data", where, param, callback => {
            if (callback.IsSuccess())
                Debug.Log($"[����] tierScore {newScore} ���� �Ϸ�");
            else
                Debug.LogWarning($"[����] tierScore ���� ����: {callback}");
        });
    }

    private IEnumerator AnimateScoreAndTier(int prevScore, int finalScore) {
        int beforeTier = TierManager.Instance.GetTierInfo(prevScore).Index;
        int afterTier = TierManager.Instance.GetTierInfo(finalScore).Index;
        int curTier = beforeTier;
        tierImage.sprite = tierSprites[curTier];
        tierImage.transform.localScale = Vector3.one;

        float elapsed = 0f;
        int displayScore = prevScore;
        float duration = scoreCountDuration;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            displayScore = Mathf.RoundToInt(Mathf.Lerp(prevScore, finalScore, t));
            scoreText.text = displayScore.ToString();

            int newTier = TierManager.Instance.GetTierInfo(displayScore).Index;
            if (newTier != curTier) {
                // Ƽ�� ���� + Punch
                curTier = newTier;
                tierImage.sprite = tierSprites[curTier];
                tierImage.transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 10, 1f);
            }

            yield return null;
        }

        // ������ �� ����
        scoreText.text = finalScore.ToString();
        int lastTier = TierManager.Instance.GetTierInfo(finalScore).Index;
        tierImage.sprite = tierSprites[lastTier];

        // ���������� �� �� �� ��ġ
        tierImage.transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 10, 1f);
        
    }


    public int GetScoreDeltaByRank(int rank) {
        switch (rank) {
            case 1: return 20;
            case 2: return 15;
            case 3: return 10;
            case 4: return 6;
            case 5: return -5;
            case 6: return -10;
            case 7: return -15;
            case 8: return -20;
            default: return 0;
        }
    }

}
