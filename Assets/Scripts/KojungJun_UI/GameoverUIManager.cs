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


    private void Awake() {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    /// <summary>
    /// 결과/랭킹 등수, 탈락자 인덱스, 내 등수(혹은 내 ClientId) 전달
    /// </summary>
    public IEnumerator StartGameoverSequence() {
        // 1. 기존 UI 처리
        scoreCanvas.SetActive(false);
        ongameCanvas.SetActive(false);
        aftergameCanvas.SetActive(true);

        // 2. 결과창 패널 애니메이션
        gameoverPanelTransform.anchoredPosition = new Vector2(0, slideOutY);
        gameoverPanelTransform.DOAnchorPosY(slideInY, 1f).SetEase(Ease.OutCubic);
        yield return new WaitForSeconds(10f);
        
        // 5. 결과창 닫기
        gameoverPanelTransform.DOAnchorPosY(slideOutY, 1f).SetEase(Ease.InCubic);
        yield return new WaitForSeconds(1f);

        MultiplayResetter.ResetAll();
        FadeManager.Instance.FadeOut();
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("TrackMake_2_Ko_Kirl_my");
    }
}
