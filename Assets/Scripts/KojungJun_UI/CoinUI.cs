using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;

    private void Start() {
        // 서버에서 불러온 후 > UI에 반영
        CurrencyManager.Instance.LoadCurrencyFromBackend();
    }

    private void UpdateUI() {
        coinText.text = CurrencyManager.Instance.Coins.ToString();
    }

    // 필요 시 외부에서 호출해서 즉시 갱신
    public void RefreshCoinUI() {
        coinText.text = CurrencyManager.Instance.Coins.ToString();
    }
}
