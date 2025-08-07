using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

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
}
