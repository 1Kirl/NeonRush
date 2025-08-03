using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using static CarShopManager;

public class CarShopManager : MonoBehaviour
{
    public static CarShopManager Instance { get; private set; }
    
    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [System.Serializable]
    public class CarInfo
    {
        public int carId;
        public GameObject carObject;
        public GameObject carBody;
        public int price;
    }

    #region Serialized

    [Header("Car Config")]
    [SerializeField] private List<CarInfo> _carInfos;

    [Header("UI")]
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private GameObject priceTextObj;
    [SerializeField] private Button confirmButton;
    [SerializeField] private ShopCameraController cameraController;
    [SerializeField] private Button equipButton;
    [SerializeField] private TMP_Text equipButtonText;

    #endregion

    #region Private

    private int _currentCarIndex = 0;

    #endregion

    private void Start() {
        CarManager.Instance.OnDataLoaded += InitializeCarShopUIFromUserData;
        buyButton.onClick.AddListener(TryBuyCar);
        equipButton.onClick.AddListener(OnClickEquipCar);
        confirmButton.onClick.AddListener(() => messagePanel.SetActive(false));
    }

    private void ShowCar(int index) {
        if (!CarManager.Instance.IsInitialized) {
            Debug.Log("[CarShop] 차량 데이터가 아직 로드되지 않았습니다.");
            return;
        }
        _currentCarIndex = index;

        // 차량 오브젝트 보여주기
        for (int i = 0; i < _carInfos.Count; i++) {
            _carInfos[i].carObject.SetActive(i == index);
        }

        var car = _carInfos[index];
        bool isUnlocked = CarManager.Instance.IsUnlocked(car.carId);
        bool isFree = car.price == 0;
        bool isEquipped = CarManager.Instance.EquippedItem == car.carId;
        // 가격 또는 무료 차량이면 버튼/텍스트 숨김
        bool shouldShowPurchaseUI = !isUnlocked && !isFree;

        priceTextObj.gameObject.SetActive(shouldShowPurchaseUI);
        buyButton.gameObject.SetActive(shouldShowPurchaseUI);

        if (shouldShowPurchaseUI) {
            priceText.text = car.price.ToString();
        }

        bool shouldShowEquipUI = isUnlocked && !shouldShowPurchaseUI;
        equipButton.gameObject.SetActive(shouldShowEquipUI);
        equipButtonText.text = isEquipped ? "Equipped" : "Equip";
        equipButton.interactable = !isEquipped;

    }




    public void ShowNextCar() {
        _currentCarIndex = (_currentCarIndex + 1) % _carInfos.Count;
        ShowCar(_currentCarIndex);
    }

    public void ShowPreviousCar() {
        _currentCarIndex = (_currentCarIndex - 1 + _carInfos.Count) % _carInfos.Count;
        ShowCar(_currentCarIndex);
    }
    public void RefreshCurrentCarUI() {
        ShowCar(_currentCarIndex);
    }

    private void TryBuyCar() {
        var car = _carInfos[_currentCarIndex];
        int carId = car.carId;
        int price = car.price;

        // 구매 확인 팝업 띄우기
        PurchasePopup.Instance.Show($"Do you want to buy this car for {price} coins?", () =>
        {
            PurchaseCar(carId, price);
        });
    }


    private void ShowMessage(string msg) {
        messageText.text = msg;
        messagePanel.SetActive(true);
    }

    private void OnClickEquipCar() {
        var car = _carInfos[_currentCarIndex];
        CarManager.Instance.EquipItem(car.carId, () =>
        {
            MainThreadDispatcher.Enqueue(() => {
                ShowCar(_currentCarIndex);
                ShowMessage("Car equipped!");
            });
        });

    }

    public void InitializeCarShopUIFromUserData() {
        if (_carInfos == null || _carInfos.Count == 0) return;
        SetCarToEquipped();

        var car = _carInfos[_currentCarIndex];
        bool isUnlocked = CarManager.Instance.IsUnlocked(car.carId);
        bool isEquipped = CarManager.Instance.EquippedItem == car.carId;

        Debug.Log($"[InitCarShop] carId: {car.carId}, isUnlocked: {isUnlocked}, isEquipped: {isEquipped}");
    }
    public int GetIndexOfEquippedCar() {
        int equippedId = CarManager.Instance.EquippedItem;
        return _carInfos.FindIndex(c => c.carId == equippedId);
    }

    public void SetCarToEquipped() {
        int index = GetIndexOfEquippedCar();
        if (index >= 0) {
            _currentCarIndex = index;
            Debug.Log($"[CarShop] 장착 차량 표시: Index {index}, CarId {_carInfos[index].carId}");
        }
        else {
            Debug.LogWarning("[CarShop] 장착 차량을 찾을 수 없음. 기본 인덱스 유지");
        }
    }

    public void ForceSetIndex(int index) {
        if (index < 0 || index >= _carInfos.Count) return;
        _currentCarIndex = index;
        ShowCar(_currentCarIndex);
    }

    public GameObject GetBodyObjectAt(int index) {
        if (index >= 0 && index < _carInfos.Count)
            return _carInfos[index].carBody;
        return null;
    }

    public void ResetAllOtherCarRotations(int exceptIndex) {
        for (int i = 0; i < _carInfos.Count; i++) {
            if (i == exceptIndex) continue;

            if (_carInfos[i].carBody != null) {
                Transform bodyTransform = _carInfos[i].carBody.transform;
                Vector3 currentEuler = bodyTransform.eulerAngles;
                bodyTransform.rotation = Quaternion.Euler(currentEuler.x, 0f, currentEuler.z);
            }
        }
    }
    private void PurchaseCar(int carId, int price) {
        CarManager.Instance.BuyItem(
            carId,
            price,
            () => CurrencyManager.Instance.TrySpendCoins(price),
            () => {
                MainThreadDispatcher.Enqueue(() => {
                    ShowMessage("Purchase successful!");
                    FindObjectOfType<CoinUI>()?.RefreshCoinUI();
                    ShowCar(_currentCarIndex);
                });
            }
        );
    }
}
