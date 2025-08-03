using UnityEngine;

public class CoinEffectManager : MonoBehaviour
{
    public static CoinEffectManager Instance;

    [SerializeField] private GameObject coinEffectPrefab; 

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayEffect(Vector3 position) {
        if (coinEffectPrefab == null) {
            Debug.LogWarning("CoinEffectManager: coinEffectPrefab이 설정되지 않았습니다.");
            return;
        }

        GameObject effect = Instantiate(coinEffectPrefab, position, Quaternion.identity);
        Destroy(effect, 1.5f); // 파티클 끝나면 자동 제거
    }
}
