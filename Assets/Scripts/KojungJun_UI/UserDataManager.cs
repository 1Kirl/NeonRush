using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// ��Ƽ ���� ��ſ� �г��� + ���� ���� ID ��ȯ
    /// </summary>
    public (string nickname, int equippedCarId, int equippedDieEffectId, int equippedTrailId) GetUserMultiplayInfo() {
        string nickname = PlayerPrefs.GetString("nickname", "Unknown");
        int equippedCarId = CarManager.Instance.EquippedItem;
        int equippedDieEffectId = DieEffectManager.Instance.EquippedItem;
        int equippedTrailId = TrailManager.Instance.EquippedItem;

        return (nickname, equippedCarId, equippedDieEffectId, equippedTrailId);
    }


}
