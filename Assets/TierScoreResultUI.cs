using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using BackEnd;

public class TierScoreResultUI : MonoBehaviour
{
    [Header("티어/점수 UI")]
    public Image tierImage;
    public Sprite[] tierSprites; // TierManager.TierTable.Count만큼
    public TMP_Text scoreText;

    [Header("연출 파라미터")]
    public float scoreCountDuration = 1.2f;
    public float punchScale = 1.2f;
    public float punchDuration = 0.35f;

    [Header("패널")]
    public GameObject rootPanel;

    Coroutine scoreRoutine;

    /// <summary>
    /// 티어/점수 연출 (점수 변화→티어 변동 자동)
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
        // 1. 현재 티어 점수 불러오기 (로컬 캐시/PlayerPrefs에서, 서버 동기화 전)
        int prevScore = PlayerPrefs.GetInt("tierScore", 0);

        // 2. 변화량 계산
        int delta = GetScoreDeltaByRank(myRank);
        int finalScore = Mathf.Max(0, prevScore + delta); // 음수 방지

        // 3. 애니메이션 실행
        PlayTierScoreResult(prevScore, finalScore);

        // 4. 로컬 캐시도 미리 업데이트 (서버 통신 전 UI 신속성 ↑)
        PlayerPrefs.SetInt("tierScore", finalScore);
        PlayerPrefs.Save();

        // 5. 서버(DB)에 최종 점수 반영 (비동기)
        UpdateTierScoreToBackend(finalScore);
    }

    /// <summary>
    /// 서버(DB)에 tierScore를 업데이트하는 함수 예시 (뒤끝 SDK 예시)
    /// </summary>
    private void UpdateTierScoreToBackend(int newScore) {
        Param param = new Param();
        param.Add("tierScore", newScore);

        Where where = new Where();
        where.Equal("owner_inDate", BackEnd.Backend.UserInDate);

        BackEnd.Backend.GameData.Update("user_data", where, param, callback => {
            if (callback.IsSuccess())
                Debug.Log($"[서버] tierScore {newScore} 저장 완료");
            else
                Debug.LogWarning($"[서버] tierScore 저장 실패: {callback}");
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
                // 티어 변경 + Punch
                curTier = newTier;
                tierImage.sprite = tierSprites[curTier];
                tierImage.transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 10, 1f);
            }

            yield return null;
        }

        // 마지막 값 고정
        scoreText.text = finalScore.ToString();
        int lastTier = TierManager.Instance.GetTierInfo(finalScore).Index;
        tierImage.sprite = tierSprites[lastTier];

        // 최종적으로 한 번 더 펀치
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
