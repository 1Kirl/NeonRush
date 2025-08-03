using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;
using DG.Tweening.Core.Easing;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{


    #region Definitions

    [Header("Score Settings")]
    [Tooltip("Multiplier applied to the distance traveled to calculate score.")]
    [SerializeField] private float _distanceMultiplier = 1.0f;

    [Tooltip("Transform of the player's car.")]
    [SerializeField] private Transform _playerTransform;

    [Tooltip("UI Canvas for displaying the score.")]
    [SerializeField] private GameObject _scoreCanvas;

    [Tooltip("Text field for showing the total score.")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private GameObject player;

    private float _totalScore = 0f;
    private Vector3 _lastPosition;
    private bool _isTracking = false;
    private int _lastDisplayedScore = 0;
    private bool _isDead = false;
    private bool _restartOnce = true;
    private Coroutine _deathCoroutine;
    private Vector2 _initialScorePosition;

    public static ScoreManager Instance;
    #endregion





    #region Properties

    // Current total score rounded to int
    public int CurrentScore => Mathf.FloorToInt(_totalScore);

    // Whether the system is currently tracking score
    public bool IsTracking => _isTracking;

    // Whether the player is in dead state
    public bool IsDead => _isDead;

    #endregion





    #region Unity Functions

    // Initializes references and sets initial score position

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        if (player == null)
        {
            Debug.Log("[Score] This is multi Scene");
            return;
        }
        if (player != null)
        {
            _playerTransform = player.transform;
        }

        _lastPosition = GetCarPosition();
        _initialScorePosition = _scoreText.rectTransform.anchoredPosition;
        _scoreCanvas.SetActive(false);
    }


    // Called every frame to update distance-based score
    private void Update() {
        if (_isTracking)
        {
            UpdateDistanceScore();
            UpdateScoreUI();
        }
    }

    #endregion





    #region Public Functions

    public void Initializing(GameObject myCar)
    {
        player = myCar;
        _playerTransform = myCar.transform;
        _lastPosition = GetCarPosition();
        _initialScorePosition = _scoreText.rectTransform.anchoredPosition;
        _scoreCanvas.SetActive(false);
    }

    // Starts score tracking and resets position/score
    public void StartTracking()
    {
        _totalScore = 0f;
        _lastPosition = GetCarPosition();
        _isTracking = true;
        _scoreCanvas.SetActive(true);
        _scoreText.rectTransform.anchoredPosition = _initialScorePosition;
    }


    // Stops tracking and triggers UI drop animation
    public void StopTracking(bool isMulti) {

        if (isMulti)
        {
            _isDead = true;
        }
        else
        {
            _isTracking = false;
            _isDead = true;
            if (_deathCoroutine != null)
            {
                StopCoroutine(_deathCoroutine);
            }

            _deathCoroutine = StartCoroutine(HandleScoreExitAndRespawnDelay());


            if (GameManager_main.Instance != null)
            {
                GameManager_main.Instance.OnGameOver(CurrentScore);

                if (GameManager_main.Instance.startButton != null)
                {
                    GameManager_main.Instance.startButton.gameObject.SetActive(true);
                }
            }
        }
    }


    // Adds a flip score to the total score
    public void AddFlipScore(int score) {
        _totalScore += score;
    }


    // Resets the dead state so score tracking can resume
    public void ResetDeathStatus()
    {
        _isDead = false;
        _restartOnce = true;
    }


    // Enables/disables score tracking manually
    public void SetTracking(bool isTracking) {
        _isTracking = isTracking;
    }

    #endregion





    #region Private Functions

    // Adds distance-based score based on car movement
    private void UpdateDistanceScore() {
        if (_isDead)
        {
            return;
        }
        else
        {
            if (_restartOnce)
            {
                _lastPosition = GetCarPosition();
                _restartOnce = false;
            }
            Vector3 currentPosition = GetCarPosition();
            float distance = Vector3.Distance(_lastPosition, currentPosition);
            _lastPosition = currentPosition;

            float scoreToAdd = distance * _distanceMultiplier;
            _totalScore += scoreToAdd;   
        }
    }


    // Updates the UI if the score has changed
    private void UpdateScoreUI() {
        int currentScore = CurrentScore;

        if (currentScore != _lastDisplayedScore) {
            _scoreText.text = currentScore.ToString("N0");
            _lastDisplayedScore = currentScore;
            PlayBounceAnimation();
        }
    }


    // Plays scale animation on score text
    private void PlayBounceAnimation() {
        _scoreText.transform.DOKill();
        _scoreText.transform.localScale = Vector3.one;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(_scoreText.transform.DOScale(1.3f, 0.1f));
        sequence.Append(_scoreText.transform.DOScale(1f, 0.15f));
    }


    // Handles UI slide out and in on death
    private IEnumerator HandleScoreExitAndRespawnDelay() {
        RectTransform rect = _scoreText.GetComponent<RectTransform>();

        yield return rect.DOAnchorPosY(-150f, 1f).SetEase(Ease.InBack).WaitForCompletion();
        yield return rect.DOAnchorPosY(300f, 1f).SetEase(Ease.OutBack).WaitForCompletion();

        _scoreCanvas.SetActive(false);

        yield return new WaitForSeconds(0.5f);
    }


    // Returns current position of the player
    private Vector3 GetCarPosition() {
        if (_playerTransform != null) {
            return _playerTransform.position;
        }

        return Vector3.zero;
    }

    #endregion

}