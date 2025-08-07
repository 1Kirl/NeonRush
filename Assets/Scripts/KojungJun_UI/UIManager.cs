using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using DG.Tweening;
using System.Collections.Generic;
using BackEnd;

public class UIManager : MonoBehaviour
{
    #region Definition

    public static UIManager Instance;

    #endregion

    #region Serialized Variables

    [Header("Panels")]
    [SerializeField] private UIPanelController _shopPanel;
    [SerializeField] private UIPanelController _collectionPanel;

    [Header("Panel Root Containers")]
    [SerializeField] private RectTransform _shopPanelRoot;

    [Header("Gameplay")]
    [SerializeField] private CinemachineVirtualCamera _carStareCamera;
    [SerializeField] private GameObject _startCylinder;
    [SerializeField] private Rigidbody _carRigid;

    [Header("Bottom Buttons")]
    [SerializeField] private Button _shopButton;
    [SerializeField] private Button _homeButton;
    [SerializeField] private Button _collectionButton;

    [Header("Highlight Settings")]
    //[SerializeField] private float _transitionDuration = 0.3f;
    [SerializeField] private CanvasGroup _homeHighlight;
    [SerializeField] private CanvasGroup _shopHighlight;
    [SerializeField] private CanvasGroup _collectionHighlight;

    [Header("Highlight")]
    [SerializeField] private ButtonHighlightController _homeButtonHighlight;
    [SerializeField] private ButtonHighlightController _shopButtonHighlight;
    [SerializeField] private ButtonHighlightController _collectionButtonHighlight;

    [Header("Collection Camera")]
    [SerializeField] private ShopCameraController _shopCameraController;
    [SerializeField] private CinemachineVirtualCamera _shopVirtualCamera;

    [Header("Collection Buttons")]
    [SerializeField] private CanvasGroup _nextButtonGroup;
    [SerializeField] private CanvasGroup _prevButtonGroup;

    [Header("Play Button")]
    [SerializeField] private GameObject _playButton;

    [Header("Shop Panel Back")]
    [SerializeField] private GameObject _ShopPanel_Background;

    [SerializeField] private GameObject _Achievement_Button;
    #endregion

    #region Private Variables

    private ButtonHighlightController _currentHighlight;
    private Dictionary<PanelType, UIPanelController> _panelMap;
    private Dictionary<PanelType, Button> _buttonMap;
    private PanelType _currentPanel;
    private float _fadeDuration = 0.5f;
    private Button _currentSelectedButton;
    private bool _isSwitchingPanel = false; // double click check.
    #endregion

    #region Unity Functions

    public void SetAchievementButtonVisible(bool visible) {
        _Achievement_Button.SetActive(visible);
    }



    private void Awake() {
        if (Instance != null && Instance != this) { 
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _panelMap = new Dictionary<PanelType, UIPanelController>
        {
            { PanelType.Shop, _shopPanel },
            { PanelType.Collection, _collectionPanel }
        };

        _buttonMap = new Dictionary<PanelType, Button>
        {
            { PanelType.Shop, _shopButton },
            { PanelType.Home, _homeButton },
            { PanelType.Collection, _collectionButton }
        };
    }

    private void Start() {
        foreach (var panel in _panelMap.Values)
            panel.InstantlyHide();

        _currentHighlight = _homeButtonHighlight;
        _currentHighlight.SetHighlighted(true);
        _homeHighlight.transform.localScale = Vector3.one * 1.2f;

        OpenPanel(PanelType.Home);
    }

    #endregion

    #region Public Functions

    public void OpenPanel(PanelType panel) {
        StartCoroutine(SwitchPanel(panel));
    }

    public void GameStart() {
        _carStareCamera.Priority = 0;
        _startCylinder.SetActive(false);
        _carRigid.isKinematic = false;

        //ScoreManager.Instance.StartTracking();
    }

    public void FlashNextButton() {
        FlashAlpha(_nextButtonGroup);
    }

    public void FlashPrevButton() {
        FlashAlpha(_prevButtonGroup);
    }

    #endregion

    #region Private Functions

    private IEnumerator SwitchPanel(PanelType target) {
        if (_isSwitchingPanel || _currentPanel == target)
            yield break;

        _isSwitchingPanel = true;
        AchievementManager.Instance.CloseAchievementPanel();

        UpdateButtonHighlight(target);
        UpdateButtonInteractivity(target);
        // 이전 패널이 Collection이고, 새로운 패널은 다른 패널일 때
        if (_currentPanel == PanelType.Collection && target != PanelType.Collection) {
            _shopCameraController.ExitCollectionPanel();  // 회전 정지 등 정리 작업
        }

        // 슬라이드 아웃 & 패널 숨기기
        if (_currentPanel == PanelType.Shop) {
            yield return AnimatePanelSlideOut();
            _shopPanelRoot.gameObject.SetActive(false);
        }
        else if (_panelMap.ContainsKey(_currentPanel)) {
            _panelMap[_currentPanel].InstantlyHide();
        }
        _currentPanel = target;
        _shopVirtualCamera.Priority = 0;
        _carStareCamera.Priority = 20;

        _playButton.SetActive(_currentPanel == PanelType.Home);
        _ShopPanel_Background.SetActive(_currentPanel == PanelType.Shop);

        if (_currentPanel == PanelType.Home) {
            _isSwitchingPanel = false;
            yield break;
        }

        if (_currentPanel == PanelType.Collection) {
            _shopVirtualCamera.Priority = 20;
            _carStareCamera.Priority = 0;

            _shopCameraController.SetCameraToEquippedInstantly();
            _collectionPanel.ShowInstantly();
            _shopCameraController.EnterCollectionPanel(); 
            _isSwitchingPanel = false;
            yield break;
        }

        if (_panelMap.TryGetValue(_currentPanel, out var panel)) {
            yield return AnimatePanelSlideIn();
            ShopCategoryManager.Instance.TriggerDefaultCategorySelection();
            panel.ShowInstantly();
        }
        _isSwitchingPanel = false;

    }
        private void UpdateButtonHighlight(PanelType panel) {
        _currentHighlight?.SetHighlighted(false);

        switch (panel) {
            case PanelType.Home:
                _currentHighlight = _homeButtonHighlight;
                break;
            case PanelType.Shop:
                _currentHighlight = _shopButtonHighlight;
                break;
            case PanelType.Collection:
                _currentHighlight = _collectionButtonHighlight;
                break;
        }

        _currentHighlight?.SetHighlighted(true);
    }

    private void UpdateButtonInteractivity(PanelType selected) {
        foreach (var pair in _buttonMap) {
            var button = pair.Value;
            var colors = button.colors;
            colors.disabledColor = Color.white;
            button.colors = colors;
            button.interactable = true;
        }

        if (_buttonMap.TryGetValue(selected, out var selectedButton)) {
            selectedButton.interactable = false;
            _currentSelectedButton = selectedButton;
        }
    }

    private void FlashAlpha(CanvasGroup canvasGroup) {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(canvasGroup.DOFade(0.6f, 0.15f));
        sequence.Append(canvasGroup.DOFade(1f, 0.15f));
    }


    private IEnumerator AnimatePanelSlideIn() {
        _shopPanelRoot.anchoredPosition = new Vector2(0, 700f);
        _shopPanelRoot.gameObject.SetActive(true);
        yield return null;
        yield return _shopPanelRoot
            .DOAnchorPos(Vector2.zero, _fadeDuration)
            .SetEase(Ease.OutCubic)
            .WaitForCompletion();
    }

    private IEnumerator AnimatePanelSlideOut() {
        yield return _shopPanelRoot
            .DOAnchorPos(new Vector2(0, 1300f), 0.3f)
            .SetEase(Ease.InCubic)
            .WaitForCompletion();
        _shopPanelRoot.gameObject.SetActive(false);
    }
    #endregion
}

