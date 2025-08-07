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
        public string name;                         // "Single", "Multi" �� ī�װ� �̸�
        public Button toggleButton;                 // ��ۿ� ��ư
        public RectTransform[] rankingRows;         // ���� ǥ�õ� ��ŷ row �г� (row�� RectTransform)
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
    /// �ݵ��! ���� ������ row ������ ���ε�(SetActive ����) �� ȣ��!
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

        // [1] ���� row�� �ٷ� ��Ȱ��ȭ + ��ġ ���� (�ִϸ��̼� ����)
        if (current != null) {
            for (int i = 0; i < current.rankingRows.Length; i++) {
                var row = current.rankingRows[i];
                if (row == null || !row.gameObject.activeSelf) continue;

                row.DOKill();
                row.anchoredPosition = new Vector2((float)1201.5, row.anchoredPosition.y);
                row.gameObject.SetActive(false);
            }
        }

        // [2] �� ī�װ� row�鸸 ����ó�� ���ʴ��(����) �����̵� ��
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

    // �ʿ��, �ڵ� ����
    private IEnumerator SelectDefaultCategoryWithDelay(string defaultCategory) {
        yield return new WaitForSeconds(0.05f);
        OnCategorySelected(defaultCategory);
    }         
    public void TriggerDefaultCategorySelection(string defaultCategory) {
        if (string.IsNullOrEmpty(currentCategory))
            StartCoroutine(SelectDefaultCategoryWithDelay(defaultCategory));
    }
}
