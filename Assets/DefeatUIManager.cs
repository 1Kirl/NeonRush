using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DefeatUIManager : MonoBehaviour
{
    [System.Serializable]
    public class RowDefeatUI
    {
        public RectTransform defeatImage;   // Defeat 이미지를 가진 오브젝트 (row별)
    }

    public RowDefeatUI[] rowDefeatUIs;      // 랭킹 Row 수(최대 8개)에 맞춰서 인스펙터에 할당
    public float slideFromX = -600f;        // 애니메이션 시작 위치 (왼쪽 밖)
    public float targetX = 0f;              // 도착 위치
    public float slideDuration = 0.6f;      // 애니메이션 지속 시간
    public float defeatImageFade = 1f;      // 추가 효과(투명도 등)

    /// <summary>
    /// 탈락 인덱스만 골라 애니메이션!
    /// </summary>
    public void PlayDefeatForLosers(List<int> defeatIndexes) {
        foreach (var idx in defeatIndexes) {
            if (idx < 0 || idx >= rowDefeatUIs.Length) continue;

            var defeatRect = rowDefeatUIs[idx].defeatImage;
            if (defeatRect == null) continue;

            // 애니메이션 전: 왼쪽에서 시작, 안 보이게
            defeatRect.gameObject.SetActive(true);
            defeatRect.anchoredPosition = new Vector2(slideFromX, defeatRect.anchoredPosition.y);
            defeatRect.GetComponent<CanvasGroup>()?.DOFade(defeatImageFade, 0); // 옵션

            // DOTween: 좌→우 슬라이드 (targetX), fade in (옵션)
            Sequence seq = DOTween.Sequence();
            seq.Append(defeatRect.DOAnchorPosX(targetX, slideDuration).SetEase(Ease.OutBack));
            // fade in 연출을 원하면 seq.Join(defeatRect.GetComponent<CanvasGroup>().DOFade(1, slideDuration));
        }
    }

    /// <summary>
    /// 모든 Defeat 이미지 숨기기 (패널 리셋시)
    /// </summary>
    public void HideAllDefeat() {
        foreach (var row in rowDefeatUIs)
            if (row.defeatImage != null)
                row.defeatImage.gameObject.SetActive(false);
    }
}
