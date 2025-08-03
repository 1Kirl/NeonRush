using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    [SerializeField] private Image fadeOverlay; // ȭ�� ��ü�� ���� ���� �̹���

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        // ���� �� ������ �����ϰ�
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
        FadeManager.Instance.SetBlackInstant();          // �������� ����
        yield return new WaitForSeconds(0.2f);

        FadeManager.Instance.FadeIn(1.5f);               // ���̵� ��
        yield return new WaitForSeconds(1.5f);

        // ī��Ʈ�ٿ�
        StartCoroutine(FinishCountdownManager.Instance.StartStartCountdown());
    }
}
