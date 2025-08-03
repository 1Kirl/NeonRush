/*using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreUI : ScoreManager
{


    #region Serialized Fields

    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private ScoreManager _scoreManager;

    #endregion





    #region Private Fields

    private int _lastDisplayedScore = 0;

    #endregion


    #region Unity Functions

    private void Update() {
        if (!_scoreManager.IsTracking) return;

        int currentScore = Mathf.FloorToInt(_scoreManager.CurrentScore);

        if (currentScore != _lastDisplayedScore) {
            _scoreText.text = currentScore.ToString("N0");
            _lastDisplayedScore = currentScore;

            PlayBounceAnimation();
        }
    }

    private void PlayBounceAnimation() {
        _scoreText.transform.DOKill(); // 이전 애니메이션 중지
        _scoreText.transform.localScale = Vector3.one; // 원래 크기

        Sequence sequence = DOTween.Sequence();
        sequence.Append(_scoreText.transform.DOScale(1.3f, 0.1f));
        sequence.Append(_scoreText.transform.DOScale(1f, 0.15f));
    }

    #endregion
}
*/