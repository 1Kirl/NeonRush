using BackEnd;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    #region Singleton
    public static CurrencyManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    #endregion

    #region Variables
    private int _coins = 0; // 초기값. 후에 Load로 대체
    public int Coins => _coins;

    #endregion

    #region Public Methods

    public bool TrySpendCoins(int amount) {
        if (_coins < amount) return false;
        _coins -= amount;
        int spent = PlayerPrefs.GetInt("coin_spent", 0) + amount;
        PlayerPrefs.SetInt("coin_spent", spent);
        PlayerPrefs.Save();
        Debug.Log($"[업적] 코인 소비 누적: {spent}");
        UpdateUserDataToServer("coin_spent", spent);

        SaveCurrencyToBackend(); // 선택사항
        return true;
    }

    public void AddCoins(int amount) {
        _coins += amount;
        SaveCurrencyToBackend();
        MainThreadDispatcher.Enqueue(() => {
            FindObjectOfType<CoinUI>()?.RefreshCoinUI();
        });
    }

    public void LoadCurrencyFromBackend() {
        Backend.GameData.GetMyData("user_data", new Where(), callback =>
        {
            if (callback.IsSuccess()) {
                var row = callback.FlattenRows()[0];
                if (row.ContainsKey("coins")) {
                    _coins = int.Parse(row["coins"].ToString());

                    // 로드 완료 후 UI 갱신
                    MainThreadDispatcher.Enqueue(() => {
                        FindObjectOfType<CoinUI>()?.RefreshCoinUI();
                    });

                }
            }
        });
    }
    public void SaveCoinsToBackend() {
        Param param = new Param { { "coins", _coins } };
        Backend.GameData.Update("user_data", new Where(), param, callback => {
            if (callback.IsSuccess())
                Debug.Log($"[CurrencyManager] coins 저장 완료: {_coins}");
            else
                Debug.LogWarning("[CurrencyManager] coins 저장 실패");
        });
    }

    private void SaveCurrencyToBackend() {
        Param param = new Param { { "coins", _coins } };
        Backend.GameData.Update("user_data", new Where(), param, callback => {
            Debug.Log("재화 저장 완료");
        });
    }
    private void UpdateUserDataToServer(string key, int value) {
        Param param = new Param();
        param.Add(key, value);

        Where where = new Where();
        where.Equal("owner_inDate", Backend.UserInDate);

        Backend.GameData.Update("user_data", where, param, callback =>
        {
            if (callback.IsSuccess())
                Debug.Log($"[서버] {key} 저장 완료: {value}");
            else
                Debug.LogWarning($"[서버] {key} 저장 실패: {callback}");
        });
    }
    #endregion
}
