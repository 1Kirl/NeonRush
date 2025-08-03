using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ShopCategoryManager : MonoBehaviour
{
    [System.Serializable]
    public class Category
    {
        public string name;
        public GameObject button;
        public RectTransform[] contentPanels;
    }

    public Category[] categories;
    public float punchScale = 0.2f;
    public float punchDuration = 0.3f;
    public float slideDistance = 1500f;
    public float slideDuration = 0.35f;

    private string currentCategory = "";
    private bool isAnimating = false;

    public static ShopCategoryManager Instance;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OnCategorySelected(string newCategory) {
        if (isAnimating || currentCategory == newCategory)
            return;

        isAnimating = true; // 애니메이션 중임을 표시
        SetAllButtonsInteractable(false); // 버튼 잠금

        Category current = null;
        Category next = null;

        foreach (var category in categories) {
            if (category.name == currentCategory) current = category;
            if (category.name == newCategory) next = category;
        }

        foreach (var category in categories) {
            var tr = category.button.transform;
            tr.DOKill();

            if (category.name == newCategory) {
                tr.DOScale(new Vector3(2f, 2f, 1f), 0.3f).SetEase(Ease.OutBack);
            }
            else {
                tr.DOScale(new Vector3(1.5f, 1.5f, 1f), 0.25f).SetEase(Ease.OutSine);
            }
        }

        Sequence sequence = DOTween.Sequence();

        if (current != null) {
            foreach (var panel in current.contentPanels) {
                if (panel == null) continue;

                panel.DOKill();
                sequence.Join(panel.DOAnchorPosX(1500, slideDuration).SetEase(Ease.InBack)
                    .OnComplete(() => panel.gameObject.SetActive(false)));
            }
        }

        if (next != null) {
            foreach (var panel in next.contentPanels) {
                if (panel == null) continue;

                panel.DOKill();
                sequence.AppendCallback(() => {
                    panel.gameObject.SetActive(true);
                    panel.anchoredPosition = new Vector2(1500, panel.anchoredPosition.y);
                });
                sequence.Append(panel.DOAnchorPosX(-500, slideDuration).SetEase(Ease.OutExpo));
            }
        }

        // 애니메이션 완료 시 버튼 다시 활성화
        sequence.OnComplete(() => {
            currentCategory = newCategory;
            isAnimating = false;
            SetAllButtonsInteractable(true); // 버튼 다시 켜기
        });
    }

    private void SetAllButtonsInteractable(bool interactable) {
        foreach (var category in categories) {
            var btn = category.button.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
                btn.interactable = interactable;
        }
    }

    private IEnumerator SelectDefaultCategoryWithDelay() {
        yield return new WaitForSeconds(0.5f);
        OnCategorySelected("DieEffect");
    }

    public void TriggerDefaultCategorySelection() {
        if (string.IsNullOrEmpty(currentCategory))
            StartCoroutine(SelectDefaultCategoryWithDelay());
    }
}
