using DG.Tweening;
using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using System.Collections.Generic;

public class FlipScoreManager : MonoBehaviour 
{


    #region SerializeField variables

    [Header("Total Score UI")]
    [SerializeField] private RectTransform _scoreTextTransform;

    [Header("Flip Score Settings")]
    [SerializeField] private GameObject _floatingTextPrefab;
    [SerializeField] private Transform _carTransform;
    [SerializeField] private GameObject _floatingTextEffect;
    Queue<ParticleSystem> _effectQueue;
    [SerializeField] private WorldCanvasController _worldCanvasController;

    [Header("Dependencies")]
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private FlipChecker _flipChecker;

    [Header("Bonus Text")]
    [SerializeField] private TextMeshProUGUI _bonusText;
    [SerializeField] private RectTransform _bonusTextTransform;

    [Header("Trick Name Display")]
    [SerializeField] private TextMeshProUGUI _trickNameText;
    [SerializeField] private CanvasGroup _trickNameCanvasGroup;
    [SerializeField] private FlipText _flipText;

    #endregion





    #region private variables

    private GameObject _floatingTextInstance;
    private int _comboScore = 0;
    private Color _lastFlipColor = Color.white;
    private readonly Color[] comboColors = new Color[]
    {
        Color.green,
        new Color(1f, 0.8f, 0f),
        new Color(1f, 0.5f, 0f),
        Color.red
    };

    #endregion





    #region Unity funcs

    private void Awake() {
        if (_flipChecker != null) {
            _flipChecker.onBackFlip.AddListener(flips => AddCustomScore(10 * flips, "Back Flip!", flips));
            _flipChecker.onFrontFlip.AddListener(flips => AddCustomScore(10 * flips, "Front Flip!", flips));
            _flipChecker.onLeftSpin.AddListener(flips => AddCustomScore(30 * flips, "Left Spin!", flips));
            _flipChecker.onRightSpin.AddListener(flips => AddCustomScore(30 * flips, "Right Spin!", flips));
            _flipChecker.onLeftRoll.AddListener(flips => AddCustomScore(40 * flips, "Left Roll!", flips));
            _flipChecker.onRightRoll.AddListener(flips => AddCustomScore(40 * flips, "Right Roll!", flips));
            _flipChecker.onCarGrounded.AddListener(OnGrounded);
            _flipChecker.onCarAirborne.AddListener(OnAirborne);
        }
        if(_floatingTextEffect!= null)
        {
            _effectQueue = new Queue<ParticleSystem>();
            for (int i = 0; i < 5; i++)
            {
                GameObject effect = Instantiate(_floatingTextEffect);
                effect.SetActive(false);
                _effectQueue.Enqueue(effect.GetComponent<ParticleSystem>());
            }
        }
    }

    #endregion





    #region public funcs
    // Called when a flip is detected and score needs to be added
    public void AddCustomScore(int score, string trickName, int flips) {
        _comboScore += score;

        string displayText = $"+{_comboScore}";
        // Display cumulative flip score next to the car
        ShowCustomFloatingScore(displayText, _comboScore);
        // Determine score-based color and forward to trick text UI
        Color color = GetColorByScore(score);
        _flipText.SetColor(color);
        _flipText.ShowTrick(trickName);
    }
    public void InitFilpScoreManager(GameObject myCar)
    {
        _carTransform = myCar.transform;
        _flipChecker = myCar.GetComponent<FlipChecker>();
        if (_flipChecker != null)
        {
            _flipChecker.onBackFlip.AddListener(flips => AddCustomScore(10 * flips, "Back Flip!", flips));
            _flipChecker.onFrontFlip.AddListener(flips => AddCustomScore(10 * flips, "Front Flip!", flips));
            _flipChecker.onLeftSpin.AddListener(flips => AddCustomScore(30 * flips, "Left Spin!", flips));
            _flipChecker.onRightSpin.AddListener(flips => AddCustomScore(30 * flips, "Right Spin!", flips));
            _flipChecker.onLeftRoll.AddListener(flips => AddCustomScore(40 * flips, "Left Roll!", flips));
            _flipChecker.onRightRoll.AddListener(flips => AddCustomScore(40 * flips, "Right Roll!", flips));
            _flipChecker.onCarGrounded.AddListener(OnGrounded);
            _flipChecker.onCarAirborne.AddListener(OnAirborne);
        }
    }

    #endregion





    #region internal funcs

    internal void ResetComboScore()
    {
        _comboScore = 0;
        _floatingTextInstance = null;

    }

    #endregion





    #region private funcs

    // Called when car touches the ground
    private void OnGrounded() {
        // If flips were made, show bonus UI and schedule final score addition
        if (_comboScore > 0) {
            ShowBonusScoreUI(_comboScore);
        }

        _scoreManager.SetTracking(true);
    }


    private void OnAirborne() {
        _scoreManager.SetTracking(false);
    }


    // Display floating trick score next to car in 3D space
    private void ShowCustomFloatingScore(string text, int combo) {
        if (_floatingTextInstance != null) {
            DOTween.Kill(_floatingTextInstance.transform);
            Destroy(_floatingTextInstance);
            _floatingTextInstance = null;
        }

        Vector3 spawnPos = _carTransform.position + _carTransform.right * 2f + Vector3.up * 1.5f;

        var instance = _floatingTextInstance = Instantiate(_floatingTextPrefab, spawnPos, Quaternion.identity);
        instance.transform.LookAt(Camera.main.transform);
        instance.transform.forward = -instance.transform.forward;

        if(_floatingTextEffect!= null)
        {
            ParticleSystem effect = _effectQueue.Dequeue();
            effect.transform.position = spawnPos;
            effect.gameObject.SetActive(true);
            effect.Play();
            _effectQueue.Enqueue(effect);
        }
        SoundManager.Instance.PlaySFX(SFXType.Flip);
        _worldCanvasController.ShowEffect();
        TextMeshPro textComp = instance.GetComponent<TextMeshPro>();
        if (textComp != null) {
            int colorIndex = Mathf.Clamp(combo / 10, 0, comboColors.Length - 1);
            _lastFlipColor = comboColors[colorIndex];

            Color startColor = _lastFlipColor;
            startColor.a = 1f;
            textComp.color = startColor;
            textComp.text = text;

            Color targetColor = startColor;
            targetColor.a = 0f;
            textComp.DOColor(targetColor, 1f).SetEase(Ease.OutQuad);
        }

        var t = instance.transform;

        Sequence seq = DOTween.Sequence();
        seq.Append(t.DOPunchScale(Vector3.one * 0.3f, 0.5f, 5, 1f));
        seq.Join(t.DOMoveY(spawnPos.y + 1.2f, 1.5f).SetEase(Ease.OutQuad));
        seq.OnComplete(() => {
            Destroy(instance);
            if (_floatingTextInstance == instance)
                _floatingTextInstance = null;
        });
    }


    // Display animated "+score" UI that slides and fades out
    private async void ShowBonusScoreUI(int bonus) {
        if (_bonusText == null || _bonusTextTransform == null) return;

        _bonusText.text = $"+{bonus}";

        Color bonusColor = _lastFlipColor;
        bonusColor.a = 1f;
        _bonusText.color = bonusColor;

        _bonusTextTransform.anchoredPosition = new Vector2(100f, -60f);
        _bonusText.gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Append(_bonusTextTransform.DOAnchorPosX(-20f, 1f).SetEase(Ease.OutQuad));

        await seq.AsyncWaitForCompletion();

        _bonusText.gameObject.SetActive(false);
        _bonusText.color = bonusColor;

        if (_scoreManager.IsDead) {
            Debug.Log("Player is dead. Bonus score not applied.");
            _comboScore = 0;
            _floatingTextInstance = null;
            return;
        }

        _scoreManager.AddFlipScore(_comboScore);
        _comboScore = 0;
        _floatingTextInstance = null;
    }

    // Helper to assign flip score color
    private Color GetColorByScore(int score) {
        if (score < 10) return Color.green;
        if (score < 20) return new Color(1f, 0.8f, 0f);
        if (score < 30) return new Color(1f, 0.5f, 0f);
        return Color.red;
    }
    public void ResetAllComboData() {
        ResetComboScore();

        if (_flipChecker != null) {
            var comboManager = FindObjectOfType<TrickComboManager>();
            comboManager?.ClearTricks();
        }
    }

    #endregion
}
