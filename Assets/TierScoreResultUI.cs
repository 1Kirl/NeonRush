using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

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
    public void PlayTierScoreResult(int prevScore, int finalScore) {
        rootPanel.SetActive(true);

        if (scoreRoutine != null)
            StopCoroutine(scoreRoutine);
        scoreRoutine = StartCoroutine(AnimateScoreAndTier(prevScore, finalScore));
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
}
