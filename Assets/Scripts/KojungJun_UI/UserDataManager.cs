using BackEnd;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유저 데이터(티어점수, 닉네임, 장착정보 등) + 티어점수 저장/불러오기 + 랭킹서버 연동
/// </summary>
public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // (필요시)
            LoadMyTierScoreFromBackend(score => {
                Debug.Log($"[UserData] Awake에서 내 티어점수 동기화: {score}");
                // 여기서 UI 최초 갱신 등도 가능!
            });
        }
        else {
            Destroy(gameObject);
        }
    }

    /// <summary> 내 티어 점수(메모리 캐싱) </summary>
    public int myTierScore = 0;

    /// <summary>
    /// (멀티 서버 전송용) 내 닉네임 및 장착 정보 반환
    /// </summary>
    public (string nickname, int equippedCarId, int equippedDieEffectId, int equippedTrailId) GetUserMultiplayInfo() {
        string nickname = PlayerPrefs.GetString("nickname", "Unknown");
        int equippedCarId = CarManager.Instance.EquippedItem;
        int equippedDieEffectId = DieEffectManager.Instance.EquippedItem;
        int equippedTrailId = TrailManager.Instance.EquippedItem;

        return (nickname, equippedCarId, equippedDieEffectId, equippedTrailId);
    }

    /// <summary>
    /// 게임 종료 시: 라운드별 등수 → 티어점수 누적/적용 + 서버저장
    /// </summary>
    public void ApplyTierScoreAndUpdateBackend(List<int> myRanksPerRound) {
        int addScore = TierManager.Instance.CalcFinalTierScore(myRanksPerRound);
        int finalScore = myTierScore + addScore;
        if (finalScore < 0) finalScore = 0;

        var tierInfo = TierManager.Instance.GetTierInfo(finalScore);
        Debug.Log($"[티어] 최종 티어점수: {finalScore} / {tierInfo.Name}");

        UpdateTierScoreToBackend(finalScore);
        myTierScore = finalScore;
    }

    /// <summary>
    /// 서버에서 내 티어 점수 불러오기(콜백 내에서 myTierScore 업데이트)
    /// </summary>
    public void LoadMyTierScoreFromBackend(Action<int> onLoaded) {
        Backend.GameData.GetMyData("user_data", new Where(), callback => {
            if (callback.IsSuccess()) {
                var row = callback.FlattenRows()[0];
                int tierScore = 0;
                if (row.ContainsKey("tierScore") && int.TryParse(row["tierScore"].ToString(), out int ts))
                    tierScore = ts;
                myTierScore = tierScore;
                Debug.Log($"[서버] 내 tierScore: {myTierScore}");
                onLoaded?.Invoke(myTierScore);
            }
            else {
                Debug.LogWarning("[서버] tierScore 불러오기 실패: " + callback);
                onLoaded?.Invoke(0);
            }
        });
    }

    /// <summary>
    /// 티어 점수 서버에 저장
    /// </summary>
    private void UpdateTierScoreToBackend(int score) {
        Param param = new Param();
        param.Add("tierScore", score);

        Where where = new Where();
        where.Equal("owner_inDate", BackEnd.Backend.UserInDate);

        BackEnd.Backend.GameData.Update("user_data", where, param, callback => {
            if (callback.IsSuccess())
                Debug.Log("[서버] tierScore 저장 완료");
            else
                Debug.LogWarning("[서버] tierScore 저장 실패: " + callback);
        });
    }
}
