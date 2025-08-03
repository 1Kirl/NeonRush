using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;

    private void Start() {
        // �������� �ҷ��� �� > UI�� �ݿ�
        CurrencyManager.Instance.LoadCurrencyFromBackend();
    }

    private void UpdateUI() {
        coinText.text = CurrencyManager.Instance.Coins.ToString();
    }

    // �ʿ� �� �ܺο��� ȣ���ؼ� ��� ����
    public void RefreshCoinUI() {
        coinText.text = CurrencyManager.Instance.Coins.ToString();
    }
}
