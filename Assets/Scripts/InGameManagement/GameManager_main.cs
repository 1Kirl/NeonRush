using BackEnd;
using TMPro;
using UnityEngine;
using BackEnd.Game;
using DG.Tweening;
public class GameManager_main : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public GameObject startButton;
    [SerializeField] private GameObject maxScoreUI;
    [SerializeField] private TextMeshProUGUI maxScoreText;
    [SerializeField] private TMP_Text coinText;
    private TrackManager trackManager;
    public static GameManager_main Instance;
    public GameObject StartCylinder;
    public GameObject Canvas;
    public GameObject myCar;
    public Transform CarSpawnPoint;
    public Rigidbody CarRigidBody;

    private int stageCurrency = 0;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    private void Start() {
        trackManager = FindObjectOfType<TrackManager>();
        int maxScore = PlayerPrefs.GetInt("max_score", 0);
        maxScoreText.text = $"Max Score: {maxScore:N0}";
        maxScoreUI.SetActive(true);
    }

    public void GameStart()
    {
        StartCylinder.SetActive(false);
        Canvas.SetActive(false);
        myCar.transform.SetParent(null);
        myCar.transform.SetPositionAndRotation(CarSpawnPoint.position, CarSpawnPoint.rotation);
        myCar.GetComponent<CarDieCheck>().isGameStarted = true;
        CarRigidBody.isKinematic = false;
        maxScoreUI.SetActive(false);
        startButton.SetActive(false);
        trackManager.BeginTrackSpawning();
        myCar.GetComponent<CarEffectController>().ResetTrail();
    }
    public void OnGameOver(int finalScore) {
        int currentMax = PlayerPrefs.GetInt("max_score", 0);
        int previous = CurrencyManager.Instance.Coins;
        int earned = stageCurrency;
        TrackPoolManager.Instance.InitializePools();
        if (finalScore > currentMax) {
            PlayerPrefs.SetInt("max_score", finalScore);
            PlayerPrefs.Save();

            Debug.Log($"New max score saved: {finalScore}");

            Param param = new Param();
            param.Add("max_score", finalScore);
            Where where = new Where();
            where.Equal("owner_inDate", Backend.UserInDate);

            Backend.GameData.Update("user_data", where, param, updateCallback =>
            {
                if (updateCallback.IsSuccess()) {
                    Debug.Log("최고 점수 업데이트 성공!");
                    // 업적 UI 갱신
                    AchievementManager.Instance?.RefreshProgress();
                }
                else
                    Debug.LogError("최고 점수 업데이트 실패: " + updateCallback);
            });

        }
        else {
            Debug.Log($"Final Score {finalScore} < Max {currentMax}, 저장 및 업로드 생략");
        }
        CurrencyManager.Instance.AddCoins(earned);
        AnimateScoreCounter(coinText, previous, previous + earned);
        stageCurrency = 0;
    }
    public void AddToStageCurrency(int amount) {
        stageCurrency += amount;
    }

    private void AnimateScoreCounter(TMP_Text text, int from, int to) {
        DOVirtual.Int(from, to, 1.5f, value => {
            text.text = value.ToString();
        })
        .SetDelay(2f)
        .SetEase(Ease.OutCubic)
        .OnComplete(() => {
            text.color = Color.yellow;
            text.transform.DOScale(Vector3.one * 1.1f, 0.2f).SetLoops(2, LoopType.Yoyo);
            text.transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 6, 0.5f);
            text.color = Color.white;
        });
    }
}

