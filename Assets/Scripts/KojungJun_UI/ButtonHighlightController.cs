using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButtonHighlightController : MonoBehaviour
{
    #region Serialized Variables

    [SerializeField] private Image _iconImage;
    [SerializeField] private Color _highlightColor = new Color(0f, 1f, 1f); // #00FFFF
    [SerializeField] private float _fadeDuration = 0.3f;

    #endregion






    #region Private Variables

    private Color _defaultColor = Color.white;

    #endregion





    #region Public Functions

    public void SetHighlighted(bool isOn) {
        if (_iconImage == null) return;

        _iconImage.DOKill(); // DOTween 애니메이션 중복 방지

        if (isOn) {
            Color targetColor = isOn ? _highlightColor : _defaultColor;
            _iconImage.DOColor(_highlightColor, _fadeDuration);
            _iconImage.transform.DOScale(1f, _fadeDuration).SetEase(Ease.OutBack);
        }
        else {
            _iconImage.DOColor(_defaultColor, _fadeDuration);
            _iconImage.transform.DOScale(0.7f, _fadeDuration).SetEase(Ease.OutQuad);
        }
    }

    #endregion
}
