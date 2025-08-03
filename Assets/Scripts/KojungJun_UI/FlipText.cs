using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Linq;

public class FlipText : MonoBehaviour
{
    #region Serialized Fields

    [Header("UI References")]
    [SerializeField] private List<TextMeshProUGUI> _trickTextPool;
    [SerializeField] private List<CanvasGroup> _canvasGroupPool;
    [SerializeField] private List<RectTransform> _transformPool;

    [Header("Fade Settings")]
    [SerializeField] private float _fadeOutDuration = 0.4f;

    [Header("Position Settings")]
    [SerializeField] private float _moveUpDistance = 100f;
    [SerializeField] private float _moveDownDistance = 120f;
    [SerializeField] private int _maxTricks = 5;

    [Header("Punch Settings")]
    [SerializeField] private float _punchScale = 0.25f;
    [SerializeField] private float _punchDuration = 0.4f;
    [SerializeField] private int _punchVibrato = 5;

    [Header("Text Appearance")]
    [SerializeField] private float _baseFontSize = 70f;
    [SerializeField] private Color _defaultColor = Color.white;

    #endregion

    #region Private Fields

    private Queue<int> _activeIndices = new Queue<int>();
    private int _currentIndex = 0;
    private Vector2 _initialPosition;

    private Dictionary<string, (int index, int count)> _activeTrickMap = new Dictionary<string, (int, int)>();
    private Dictionary<int, Tween> _activeFadeSequences = new Dictionary<int, Tween>();

    #endregion

    #region Unity Callbacks

    private void Start() {
        if (_transformPool.Count > 0) {
            _initialPosition = _transformPool[0].anchoredPosition;
        }
    }

    #endregion

    #region Public Functions

    public void ShowTrick(string trickName) {
        if (_activeTrickMap.ContainsKey(trickName)) {
            var (index, count) = _activeTrickMap[trickName];

            if (_canvasGroupPool[index].alpha > 0f) {
                count++;
                _activeTrickMap[trickName] = (index, count);
                UpdateText(index, trickName, count);
                RestartFadeOut(index);
                return;
            }
            else {
                _activeTrickMap.Remove(trickName);
            }
        }

        if (_activeIndices.Count >= _maxTricks) {
            int oldIdx = _activeIndices.Dequeue();
            RemoveTrickByIndex(oldIdx);
            HideImmediately(oldIdx);
        }

        int idx = _currentIndex % _trickTextPool.Count;
        _currentIndex++;

        Vector2 newPos;
        if (_activeIndices.Count == 0) {
            newPos = _initialPosition;
        }
        else {
            int lastIdx = _activeIndices.Last();
            Vector2 lastPos = _transformPool[lastIdx].anchoredPosition;
            newPos = lastPos + new Vector2(0f, -_moveDownDistance);
        }

        _transformPool[idx].anchoredPosition = newPos;
        _transformPool[idx].localScale = Vector3.one;

        _activeIndices.Enqueue(idx);
        _activeTrickMap[trickName] = (idx, 1);

        UpdateText(idx, trickName, 1);
        AnimateVerticalFadeUp(idx); // 포함됨
        AnimatePunch(idx);          // 펀치 애니메이션 추가
    }

    public void SetColor(Color color) {
        _defaultColor = color;
    }

    public void ResetAll() {
        foreach (var tween in _activeFadeSequences.Values)
            tween.Kill();
        _activeFadeSequences.Clear();

        foreach (var text in _trickTextPool)
            text.gameObject.SetActive(false);
        foreach (var group in _canvasGroupPool) {
            group.DOKill();
            group.alpha = 0;
        }
        foreach (var rt in _transformPool)
            rt.DOKill();

        _activeTrickMap.Clear();
        _activeIndices.Clear();
    }

    #endregion

    #region Private Functions

    private void UpdateText(int index, string name, int count) {
        var text = _trickTextPool[index];
        text.gameObject.SetActive(true);

        if (count > 1)
            text.text = $"{name} <color={GetCountColor(count)}>x{count}</color>";
        else
            text.text = name;

        text.fontSize = _baseFontSize + Mathf.Min((count - 1) * 2f, 10f);
        text.color = _defaultColor;
        _canvasGroupPool[index].alpha = 1f;
    }

    private void AnimateVerticalFadeUp(int index) {
        var rt = _transformPool[index];
        var cg = _canvasGroupPool[index];
        var text = _trickTextPool[index];

        Vector2 targetPos = rt.anchoredPosition + new Vector2(0f, _moveUpDistance);
        Color transparent = new Color(text.color.r, text.color.g, text.color.b, 0f);

        if (_activeFadeSequences.ContainsKey(index))
            _activeFadeSequences[index].Kill();

        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOAnchorPos(targetPos, _fadeOutDuration).SetEase(Ease.OutCubic))
           .Join(cg.DOFade(0f, _fadeOutDuration))
           .Join(text.DOColor(transparent, _fadeOutDuration))
           .OnComplete(() => {
               text.gameObject.SetActive(false);
               text.fontSize = _baseFontSize;
               text.color = _defaultColor;
               _activeFadeSequences.Remove(index);
               RemoveTrickByIndex(index);
           });

        _activeFadeSequences[index] = seq;
    }

    private void AnimatePunch(int index) // 추가된 함수
    {
        var rt = _transformPool[index];
        rt.localScale = Vector3.one;
        rt.DOPunchScale(Vector3.one * _punchScale, _punchDuration, _punchVibrato).SetEase(Ease.OutBack);
    }

    private void RestartFadeOut(int index) {
        if (_activeFadeSequences.ContainsKey(index)) {
            _activeFadeSequences[index].Kill();
            AnimateVerticalFadeUp(index);
        }
    }

    private void RemoveTrickByIndex(int index) {
        string keyToRemove = null;
        foreach (var kvp in _activeTrickMap) {
            if (kvp.Value.index == index) {
                keyToRemove = kvp.Key;
                break;
            }
        }

        if (keyToRemove != null)
            _activeTrickMap.Remove(keyToRemove);

        _activeIndices = new Queue<int>(_activeIndices.Where(i => i != index));
    }

    private void HideImmediately(int index) {
        _trickTextPool[index].gameObject.SetActive(false);
        _canvasGroupPool[index].DOKill();
        _transformPool[index].DOKill();
        _canvasGroupPool[index].alpha = 0f;
        _activeFadeSequences.Remove(index);
    }

    private string GetCountColor(int count) {
        if (count >= 5) return "#FF5555";
        if (count >= 3) return "#FFA500";
        return "#FFFF00";
    }

    #endregion
}
