using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameoverUIManager : MonoBehaviour
{
    public static GameoverUIManager Instance;

    [Header("Panels")]
    public GameObject scoreCanvas;
    public GameObject ongameCanvas;
    public GameObject aftergameCanvas;

    [Header("Animation")]
    public RectTransform gameoverPanelTransform;
    public float slideInY = 0f;
    public float slideOutY = 2000f;

    [Header("Defeat")]
    public RectTransform[] defeatImages; // 결과 행 순서와 1:1 대응
    public float defeatSlideFromX = -600f;
    public float defeatSlideToX = 0f;
    public float defeatSlideDuration = 0.6f;

    private void Awake() {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    /// <summary>
    /// 결과/랭킹 등수, 탈락자 인덱스, 내 등수(혹은 내 ClientId) 전달
    /// </summary>
    public IEnumerator StartGameoverSequence(List<int> defeatIndexes, int myRankingIndex) {
        // 1. 기존 UI 처리
        scoreCanvas.SetActive(false);
        ongameCanvas.SetActive(false);
        aftergameCanvas.SetActive(true);

        // 2. 결과창 패널 애니메이션
        gameoverPanelTransform.anchoredPosition = new Vector2(0, slideOutY);
        gameoverPanelTransform.DOAnchorPosY(slideInY, 1f).SetEase(Ease.OutCubic);

        // 3. 모든 Defeat 이미지 끄기
        foreach (var img in defeatImages)
            if (img != null)
                img.gameObject.SetActive(false);

        // 4. 탈락한 유저들만 Defeat 이미지 애니메이션
        foreach (int idx in defeatIndexes) {
            if (idx < 0 || idx >= defeatImages.Length) continue;

            var defeatImg = defeatImages[idx];
            if (defeatImg == null) continue;

            defeatImg.gameObject.SetActive(true);
            defeatImg.anchoredPosition = new Vector2(defeatSlideFromX, defeatImg.anchoredPosition.y);
            defeatImg.DOKill();
            defeatImg.DOAnchorPosX(defeatSlideToX, defeatSlideDuration).SetEase(Ease.OutBack);
        }

        yield return new WaitForSeconds(10f);

        // 5. 결과창 닫기
        gameoverPanelTransform.DOAnchorPosY(slideOutY, 1f).SetEase(Ease.InCubic);
        yield return new WaitForSeconds(1f);

        // 6. 탈락자만 로비로 이동
        if (defeatIndexes.Contains(myRankingIndex)) {
            MultiplayResetter.ResetAll();
            FadeManager.Instance.FadeOut();
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene("TrackMake_2_Ko_Kirl_my");
        }
        // else (승자 로직 추가 예정)
    }

    public static List<int> GetDefeatIndexes(List<ResultEntry> results, int round, int initialPlayerCount) {
        List<int> defeatIndexes = new List<int>();

        // (예시) 1라운드: 8->4명, 2라운드: 4->2명, 3라운드: 2->1명
        int count = results.Count;

        if (round == 1) {
            // 1라운드: 5등~꼴찌 전원 탈락
            for (int i = 4; i < count; i++)
                defeatIndexes.Add(i);
        }
        else if (round == 2) {
            // 2라운드: (3,4등) 탈락
            if (count >= 4) {
                defeatIndexes.Add(2);
                defeatIndexes.Add(3);
            }
        }
        else if (round == 3) {
            // 3라운드: 2등만 탈락
            if (count >= 2) {
                defeatIndexes.Add(1);
            }
        }
        return defeatIndexes;
    }
}
