using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextBlink : MonoBehaviour
{


    #region Definitions

    private TextMeshProUGUI _tmp;
    private readonly Color _neonStartColor = new Color32(0x00, 0xD9, 0xFF, 0xFF);
    private readonly Color _neonEndColor = new Color32(0xFF, 0x4E, 0x8B, 0xFF);

    #endregion



    #region Public Variables

    #endregion



    #region Serialized Variables

    #endregion



    #region Private Variables

    #endregion



    #region Properties

    #endregion



    #region Mono Behaviours

    // Initialize blinking animation on text component
    private void Start() {
        _tmp = GetComponent<TextMeshProUGUI>();
        _tmp.alpha = 0f;
        _tmp.color = _neonStartColor;
        _tmp.transform.localScale = Vector3.one;

        Sequence sequence = DOTween.Sequence();

        // Step 1: Initial fade-in
        sequence.Append(_tmp.DOFade(0.2f, 1f));

        // Step 2: Looping neon effect
        sequence.AppendCallback(() => {
            Sequence blink = DOTween.Sequence();

            blink.Append(_tmp.DOColor(_neonEndColor, 0.5f));
            blink.Join(_tmp.DOFade(0.8f, 0.5f));
            blink.Join(_tmp.transform.DOScale(1.1f, 0.5f));

            blink.Append(_tmp.DOColor(_neonStartColor, 0.5f));
            blink.Join(_tmp.DOFade(0.2f, 0.5f));
            blink.Join(_tmp.transform.DOScale(1.0f, 0.5f));

            blink.SetLoops(-1).SetEase(Ease.InOutSine);
        });
    }

    #endregion



    #region Public Functions

    #endregion



    #region Private Functions

    #endregion
}