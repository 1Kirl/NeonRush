using System;
using System.Collections.Generic;

/// <summary>
/// 티어 계산/구간/명칭 관리 (싱글톤, 상태 X)
/// </summary>
public class TierManager
{
    // 싱글톤 인스턴스
    private static TierManager _instance;
    public static TierManager Instance => _instance ??= new TierManager();

    // 티어 테이블(순서: 인덱스/명칭/점수구간)
    public static readonly List<TierInfo> TierTable = new()
    {
        new TierInfo(0, "Bronze 4", 0, 99),
        new TierInfo(1, "Bronze 3", 100, 199),
        new TierInfo(2, "Bronze 2", 200, 299),
        new TierInfo(3, "Bronze 1", 300, 399),
        new TierInfo(4, "Silver 4", 400, 499),
        new TierInfo(5, "Silver 3", 500, 599),
        new TierInfo(6, "Silver 2", 600, 699),
        new TierInfo(7, "Silver 1", 700, 799),
        new TierInfo(8, "Gold 4", 800, 899),
        new TierInfo(9, "Gold 3", 900, 999),
        new TierInfo(10, "Gold 2", 1000, 1099),
        new TierInfo(11, "Gold 1", 1100, 1199),
        new TierInfo(12, "Platinum 4", 1200, 1299),
        new TierInfo(13, "Platinum 3", 1300, 1399),
        new TierInfo(14, "Platinum 2", 1400, 1499),
        new TierInfo(15, "Platinum 1", 1500, 1599),
        new TierInfo(16, "Diamond 4", 1600, 1699),
        new TierInfo(17, "Diamond 3", 1700, 1799),
        new TierInfo(18, "Diamond 2", 1800, 1899),
        new TierInfo(19, "Diamond 1", 1900, 1999),
        new TierInfo(20, "Challenger", 2000, int.MaxValue),
    };

    /// <summary>
    /// 점수 → 티어 객체 반환
    /// </summary>
    public TierInfo GetTierInfo(int tierScore) {
        foreach (var t in TierTable) {
            if (tierScore >= t.MinScore && tierScore <= t.MaxScore)
                return t;
        }
        // score < 0 예외
        return TierTable[0];
    }

    /// <summary>
    /// 인덱스 → 티어 객체 반환
    /// </summary>
    public TierInfo GetTierInfoByIndex(int tierIndex) {
        if (tierIndex < 0) return TierTable[0];
        if (tierIndex >= TierTable.Count) return TierTable[TierTable.Count - 1];
        return TierTable[tierIndex];
    }

    /// <summary>
    /// 점수 → 티어 인덱스 반환
    /// </summary>
    public static int GetTierIndexByScore(int tierScore) {
        for (int i = 0; i < TierTable.Count; i++) {
            if (tierScore >= TierTable[i].MinScore && tierScore <= TierTable[i].MaxScore)
                return i;
        }
        return 0;
    }

    /// <summary>
    /// 라운드별 등수 리스트 → 최종 누적 티어점수 계산(보정점수 포함)
    /// </summary>
    public int CalcFinalTierScore(List<int> myRanksPerRound) {
        // myRanksPerRound: [1R 등수, 2R 등수, 3R 등수]
        int totalScore = 0;

        int r1 = myRanksPerRound.Count > 0 ? myRanksPerRound[0] : -1;
        int r2 = myRanksPerRound.Count > 1 ? myRanksPerRound[1] : -1;
        int r3 = myRanksPerRound.Count > 2 ? myRanksPerRound[2] : -1;

        if (r1 != -1)
            totalScore += CalcRoundScore(1, r1);

        // 2R 탈락자 보정
        if (r2 != -1) {
            int basic = CalcRoundScore(2, r2);
            if (r2 >= 3 && r2 <= 4) {
                if (r1 == 1) basic = -2;
                else if (r1 == 2) basic = -4;
            }
            totalScore += basic;
        }

        if (r3 != -1)
            totalScore += CalcRoundScore(3, r3);

        if (totalScore < 0)
            totalScore = 0;

        return totalScore;
    }

    /// <summary>
    /// 라운드/등수에 따른 점수 계산 (보정 미포함)
    /// </summary>
    private int CalcRoundScore(int round, int rank) {
        switch (round) {
            case 1:
                return rank switch {
                    1 => 18,
                    2 => 12,
                    3 => 9,
                    4 => 6,
                    5 => -5,
                    6 => -10,
                    7 => -15,
                    8 => -20,
                    _ => 0
                };
            case 2:
                return rank switch {
                    1 => 18,
                    2 => 12,
                    3 => -5,
                    4 => -10,
                    _ => 0
                };
            case 3:
                return rank switch {
                    1 => 24,
                    2 => 12,
                    _ => 0
                };
            default: return 0;
        }
    }
}

/// <summary>
/// 각 티어(등급) 정보
/// </summary>
public class TierInfo
{
    public int Index;
    public string Name;
    public int MinScore;
    public int MaxScore;
    // public string IconPath; // 필요시 확장

    public TierInfo(int index, string name, int min, int max) {
        Index = index;
        Name = name;
        MinScore = min;
        MaxScore = max;
    }
}
