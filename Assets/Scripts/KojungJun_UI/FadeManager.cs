using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [SerializeField] private Image fadeOverlay; // 화면 전체를 덮는 검정 이미지

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        // 시작 시 완전히 투명하게
        fadeOverlay.color = new Color(0, 0, 0, 0);
        fadeOverlay.raycastTarget = false;
    }

    public void FadeIn(float duration = 1.0f) {
        fadeOverlay.raycastTarget = true;   
        fadeOverlay.DOFade(0f, duration).OnComplete(() => {
            fadeOverlay.raycastTarget = false;
        });
    }

    public void FadeOut(float duration = 1.0f) {
        fadeOverlay.raycastTarget = true;
        fadeOverlay.DOFade(1f, duration);
    }

    public void SetBlackInstant() {
        fadeOverlay.color = new Color(0, 0, 0, 1);
        fadeOverlay.raycastTarget = true;
    }

    public IEnumerator SequenceBeforeCountdown() {
        FadeManager.Instance.SetBlackInstant();          // 검정으로 덮고
        yield return new WaitForSeconds(0.2f);

        FadeManager.Instance.FadeIn(1.5f);               // 페이드 인
        yield return new WaitForSeconds(1.5f);

        // 카운트다운
        StartCoroutine(FinishCountdownManager.Instance.StartStartCountdown());
    }
}
