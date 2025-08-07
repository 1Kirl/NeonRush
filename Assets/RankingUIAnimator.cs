using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RankingUIAnimator : MonoBehaviour
{
    [System.Serializable]
    public class RankingCategory
    {
        public string name;                         // "Single", "Multi" 등 카테고리 이름
        public Button toggleButton;                 // 토글용 버튼
        public RectTransform[] rankingRows;         // 실제 표시될 랭킹 row 패널 (row별 RectTransform)
    }

    public RankingCategory[] categories;
    public float buttonPunchScale = 0.2f;
    public float buttonPunchDuration = 0.3f;
    public float slideDistance = 1200f;
    public float slideDuration = 0.3f;
    public float staggerDelay = 0.06f;


    private string currentCategory = "";
    private bool isAnimating = false;

    public static RankingUIAnimator Instance;


    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 반드시! 실제 보여줄 row 데이터 바인딩(SetActive 포함) 후 호출!
    /// </summary>
    public void OnCategorySelected(string newCategory) {
        if (isAnimating || currentCategory == newCategory)
            return;

        isAnimating = true;
        SetAllButtonsInteractable(false);


        RankingCategory current = null;
        RankingCategory next = null;

        foreach (var category in categories) {
            if (category.name == currentCategory) current = category;
            if (category.name == newCategory) next = category;
        }

        Sequence sequence = DOTween.Sequence();

        // [1] 현재 row들 바로 비활성화 + 위치 복귀 (애니메이션 없음)
        if (current != null) {
            for (int i = 0; i < current.rankingRows.Length; i++) {
                var row = current.rankingRows[i];
                if (row == null || !row.gameObject.activeSelf) continue;

                row.DOKill();
                row.anchoredPosition = new Vector2((float)1201.5, row.anchoredPosition.y);
                row.gameObject.SetActive(false);
            }
        }

        // [2] 새 카테고리 row들만 기존처럼 차례대로(순차) 슬라이드 인
        if (next != null) {
            for (int i = 0; i < next.rankingRows.Length; i++) {
                var row = next.rankingRows[i];
                if (row == null || !row.gameObject.activeSelf) continue;

                row.DOKill();
                row.gameObject.SetActive(true);
                row.anchoredPosition = new Vector2(slideDistance, row.anchoredPosition.y);
                sequence.Append(
                    row.DOAnchorPosX(0, slideDuration).SetEase(Ease.OutExpo)
                        .SetDelay(i * staggerDelay)
                );
            }
        }

        sequence.OnComplete(() => {
            currentCategory = newCategory;
            isAnimating = false;
            SetAllButtonsInteractable(true);
        });
    }



    private void SetAllButtonsInteractable(bool interactable) {
        foreach (var category in categories) {
            if (category.toggleButton != null)
                category.toggleButton.interactable = interactable; 
        }
    }

    // 필요시, 자동 선택
    private IEnumerator SelectDefaultCategoryWithDelay(string defaultCategory) {
        yield return new WaitForSeconds(0.05f);
        OnCategorySelected(defaultCategory);
    }         
    public void TriggerDefaultCategorySelection(string defaultCategory) {
        if (string.IsNullOrEmpty(currentCategory))
            StartCoroutine(SelectDefaultCategoryWithDelay(defaultCategory));
    }
}
