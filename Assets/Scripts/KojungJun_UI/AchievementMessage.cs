using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class AchievementMessage : MonoBehaviour
{
    public static AchievementMessage Instance;

    [Header("Toast UI")]
    public RectTransform toastRoot; // x: -680ø° ¿÷¿Ω
    public TextMeshProUGUI messageText;

    private Vector2 hiddenPos = new Vector2(-680, 507);
    private Vector2 visiblePos = new Vector2(-120, 507);
    private float slideDuration = 0.5f;
    private float visibleDuration = 3f;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowToast(string message) {
        StopAllCoroutines();
        StartCoroutine(ToastRoutine(message));
    }

    private IEnumerator ToastRoutine(string message) {
        toastRoot.gameObject.SetActive(true);
        messageText.text = message;
        toastRoot.anchoredPosition = hiddenPos;

        //SoundManager.Instance.PlaySFX(SFXType.Popup);
        yield return toastRoot.DOAnchorPos(visiblePos, slideDuration).SetEase(Ease.OutCubic).WaitForCompletion();
        yield return new WaitForSeconds(visibleDuration);
        yield return toastRoot.DOAnchorPos(hiddenPos, slideDuration).SetEase(Ease.InCubic).WaitForCompletion();

        toastRoot.gameObject.SetActive(false);
    }
}
