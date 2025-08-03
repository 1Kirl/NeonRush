using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class TrickComboController : MonoBehaviour
{
    [SerializeField] private TMP_Text[] comboTextObjects;
    [SerializeField] private float showDuration = 2f;
    [SerializeField] private float scaleMultiplier = 3.5f;

    public void ShowRandomComboText(string comboName) {
        if (comboTextObjects.Length == 0) return;

        int index = Random.Range(0, comboTextObjects.Length);
        TMP_Text selected = comboTextObjects[index];

        selected.text = comboName;
        selected.gameObject.SetActive(true);

        // 초기 상태
        selected.transform.localScale = Vector3.one * scaleMultiplier;
        var color = selected.color;
        color.a = 1f;
        selected.color = color;

        Sequence seq = DOTween.Sequence();

        // 크기 도장처럼 팡! 했다가 축소
        seq.Append(selected.transform.DOScale(Vector3.one * (scaleMultiplier + 0.2f), 0.2f).SetEase(Ease.OutBack));
        seq.Append(selected.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutCubic));

        // 페이드 아웃은 약간 지연 후 시작
        seq.Join(selected.DOFade(0f, showDuration).SetDelay(0.1f));

        seq.OnComplete(() => selected.gameObject.SetActive(false));
    }

}
