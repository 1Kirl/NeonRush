using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 멀티 서버 통신용 닉네임 + 장착 차량 ID 반환
    /// </summary>
    public (string nickname, int equippedCarId, int equippedDieEffectId, int equippedTrailId) GetUserMultiplayInfo() {
        string nickname = PlayerPrefs.GetString("nickname", "Unknown");
        int equippedCarId = CarManager.Instance.EquippedItem;
        int equippedDieEffectId = DieEffectManager.Instance.EquippedItem;
        int equippedTrailId = TrailManager.Instance.EquippedItem;

        return (nickname, equippedCarId, equippedDieEffectId, equippedTrailId);
    }


}
