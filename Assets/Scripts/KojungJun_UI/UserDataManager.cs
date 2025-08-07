using BackEnd;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���� ������(Ƽ������, �г���, �������� ��) + Ƽ������ ����/�ҷ����� + ��ŷ���� ����
/// </summary>
public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // (�ʿ��)
            LoadMyTierScoreFromBackend(score => {
                Debug.Log($"[UserData] Awake���� �� Ƽ������ ����ȭ: {score}");
                // ���⼭ UI ���� ���� � ����!
            });
        }
        else {
            Destroy(gameObject);
        }
    }

    /// <summary> �� Ƽ�� ����(�޸� ĳ��) </summary>
    public int myTierScore = 0;

    /// <summary>
    /// (��Ƽ ���� ���ۿ�) �� �г��� �� ���� ���� ��ȯ
    /// </summary>
    public (string nickname, int equippedCarId, int equippedDieEffectId, int equippedTrailId) GetUserMultiplayInfo() {
        string nickname = PlayerPrefs.GetString("nickname", "Unknown");
        int equippedCarId = CarManager.Instance.EquippedItem;
        int equippedDieEffectId = DieEffectManager.Instance.EquippedItem;
        int equippedTrailId = TrailManager.Instance.EquippedItem;

        return (nickname, equippedCarId, equippedDieEffectId, equippedTrailId);
    }

    /// <summary>
    /// ���� ���� ��: ���庰 ��� �� Ƽ������ ����/���� + ��������
    /// </summary>
    public void ApplyTierScoreAndUpdateBackend(List<int> myRanksPerRound) {
        int addScore = TierManager.Instance.CalcFinalTierScore(myRanksPerRound);
        int finalScore = myTierScore + addScore;
        if (finalScore < 0) finalScore = 0;

        var tierInfo = TierManager.Instance.GetTierInfo(finalScore);
        Debug.Log($"[Ƽ��] ���� Ƽ������: {finalScore} / {tierInfo.Name}");

        UpdateTierScoreToBackend(finalScore);
        myTierScore = finalScore;
    }

    /// <summary>
    /// �������� �� Ƽ�� ���� �ҷ�����(�ݹ� ������ myTierScore ������Ʈ)
    /// </summary>
    public void LoadMyTierScoreFromBackend(Action<int> onLoaded) {
        Backend.GameData.GetMyData("user_data", new Where(), callback => {
            if (callback.IsSuccess()) {
                var row = callback.FlattenRows()[0];
                int tierScore = 0;
                if (row.ContainsKey("tierScore") && int.TryParse(row["tierScore"].ToString(), out int ts))
                    tierScore = ts;
                myTierScore = tierScore;
                Debug.Log($"[����] �� tierScore: {myTierScore}");
                onLoaded?.Invoke(myTierScore);
            }
            else {
                Debug.LogWarning("[����] tierScore �ҷ����� ����: " + callback);
                onLoaded?.Invoke(0);
            }
        });
    }

    /// <summary>
    /// Ƽ�� ���� ������ ����
    /// </summary>
    private void UpdateTierScoreToBackend(int score) {
        Param param = new Param();
        param.Add("tierScore", score);

        Where where = new Where();
        where.Equal("owner_inDate", BackEnd.Backend.UserInDate);

        BackEnd.Backend.GameData.Update("user_data", where, param, callback => {
            if (callback.IsSuccess())
                Debug.Log("[����] tierScore ���� �Ϸ�");
            else
                Debug.LogWarning("[����] tierScore ���� ����: " + callback);
        });
    }
}
