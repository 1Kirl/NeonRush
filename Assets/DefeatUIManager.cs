using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DefeatUIManager : MonoBehaviour
{
    [System.Serializable]
    public class RowDefeatUI
    {
        public RectTransform defeatImage;   // Defeat �̹����� ���� ������Ʈ (row��)
    }

    public RowDefeatUI[] rowDefeatUIs;      // ��ŷ Row ��(�ִ� 8��)�� ���缭 �ν����Ϳ� �Ҵ�
    public float slideFromX = -600f;        // �ִϸ��̼� ���� ��ġ (���� ��)
    public float targetX = 0f;              // ���� ��ġ
    public float slideDuration = 0.6f;      // �ִϸ��̼� ���� �ð�
    public float defeatImageFade = 1f;      // �߰� ȿ��(���� ��)

    /// <summary>
    /// Ż�� �ε����� ��� �ִϸ��̼�!
    /// </summary>
    public void PlayDefeatForLosers(List<int> defeatIndexes) {
        foreach (var idx in defeatIndexes) {
            if (idx < 0 || idx >= rowDefeatUIs.Length) continue;

            var defeatRect = rowDefeatUIs[idx].defeatImage;
            if (defeatRect == null) continue;

            // �ִϸ��̼� ��: ���ʿ��� ����, �� ���̰�
            defeatRect.gameObject.SetActive(true);
            defeatRect.anchoredPosition = new Vector2(slideFromX, defeatRect.anchoredPosition.y);
            defeatRect.GetComponent<CanvasGroup>()?.DOFade(defeatImageFade, 0); // �ɼ�

            // DOTween: �¡�� �����̵� (targetX), fade in (�ɼ�)
            Sequence seq = DOTween.Sequence();
            seq.Append(defeatRect.DOAnchorPosX(targetX, slideDuration).SetEase(Ease.OutBack));
            // fade in ������ ���ϸ� seq.Join(defeatRect.GetComponent<CanvasGroup>().DOFade(1, slideDuration));
        }
    }

    /// <summary>
    /// ��� Defeat �̹��� ����� (�г� ���½�)
    /// </summary>
    public void HideAllDefeat() {
        foreach (var row in rowDefeatUIs)
            if (row.defeatImage != null)
                row.defeatImage.gameObject.SetActive(false);
    }
}
