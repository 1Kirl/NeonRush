using DG.Tweening.Core.Easing;
using TMPro;
using UnityEngine;
using UnityEngine.Events;


public enum CoinType
{
    Bronze = 1,
    Silver = 3,
    Gold = 10
}
[System.Serializable]
//public class CoinCollectedEvent : UnityEvent<GameObject> { }

public class Coin : MonoBehaviour
{
    public CoinType coinType; // 드래그 설정 or 프리팹별 지정
    //public CoinCollectedEvent onCollected;

    [SerializeField] private float rotationSpeed = 90f;
    public GameObject floatingTextPrefab;

    private void FixedUpdate() {
        transform.Rotate(Vector3.up, rotationSpeed * Time.fixedDeltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            int value = (int)coinType;

            //  1. 재화 증가 (임시 저장)
            GameManager_main.Instance.AddToStageCurrency(value);

            // 2. +이펙트 텍스트 생성
            ShowFloatingText(value); 

            //  3. 효과 실행
            CoinEffectManager.Instance.PlayEffect(transform.position);
            SoundManager.Instance.PlaySFX(SFXType.GetCoin); //Get Coin Sound.

            // 4. 이벤트 및 제거
            //onCollected.Invoke(other.gameObject);
            Destroy(gameObject);
        }
    }

    private void ShowFloatingText(int amount) {
        if (floatingTextPrefab == null) return;

        Vector3 spawnPosition = transform.position + Vector3.up * 1f; // 코인 위로 조금
        var obj = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity);

        var text = obj.GetComponentInChildren<TMP_Text>();
        text.text = $"+{amount}";

        Destroy(obj, 1f); // 자동 제거
    }

}
