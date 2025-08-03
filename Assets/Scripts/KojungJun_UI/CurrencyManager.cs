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
    private int _coins = 0; // �ʱⰪ. �Ŀ� Load�� ��ü
    public int Coins => _coins;

    #endregion

    #region Public Methods

    public bool TrySpendCoins(int amount) {
        if (_coins < amount) return false;
        _coins -= amount;
        int spent = PlayerPrefs.GetInt("coin_spent", 0) + amount;
        PlayerPrefs.SetInt("coin_spent", spent);
        PlayerPrefs.Save();
        Debug.Log($"[����] ���� �Һ� ����: {spent}");
        UpdateUserDataToServer("coin_spent", spent);

        SaveCurrencyToBackend(); // ���û���
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

                    // �ε� �Ϸ� �� UI ����
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
                Debug.Log($"[CurrencyManager] coins ���� �Ϸ�: {_coins}");
            else
                Debug.LogWarning("[CurrencyManager] coins ���� ����");
        });
    }

    private void SaveCurrencyToBackend() {
        Param param = new Param { { "coins", _coins } };
        Backend.GameData.Update("user_data", new Where(), param, callback => {
            Debug.Log("��ȭ ���� �Ϸ�");
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
                Debug.Log($"[����] {key} ���� �Ϸ�: {value}");
            else
                Debug.LogWarning($"[����] {key} ���� ����: {callback}");
        });
    }
    #endregion
}
