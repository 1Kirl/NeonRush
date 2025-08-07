using System;
using System.Collections.Generic;

public class TierManager
{
    // 싱글톤 인스턴스
    private static TierManager _instance;
    public static TierManager Instance => _instance ??= new TierManager();

    // int: 0~20  브4~챌린저
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
        new TierInfo(13, "Platinum  3", 1300, 1399),
        new TierInfo(14, "Platinum  2", 1400, 1499),
        new TierInfo(15, "Platinum  1", 1500, 1599),
        new TierInfo(16, "Diamond 4", 1600, 1699),
        new TierInfo(17, "Diamond 3", 1700, 1799),
        new TierInfo(18, "Diamond 2", 1800, 1899),
        new TierInfo(19, "Diamond 1", 1900, 1999),
        new TierInfo(20, "Challenger", 2000, int.MaxValue),
    };

    public static TierInfo GetTierInfo(int tierScore) {
        foreach (var t in TierTable) {
            if (tierScore >= t.MinScore && tierScore <= t.MaxScore)
                return t;
        }
        // Exception: score <  0
        return TierTable[0];
    }

    // bestTier(int)  Tier name
    public static TierInfo GetTierInfoByIndex(int tierIndex) {
        if (tierIndex < 0) return TierTable[0];
        if (tierIndex >= TierTable.Count) return TierTable[TierTable.Count - 1];
        return TierTable[tierIndex];
    }

    // current Score -> int index return
    public static int GetTierIndexByScore(int tierScore) {
        for (int i = 0; i < TierTable.Count; i++) {
            if (tierScore >= TierTable[i].MinScore && tierScore <= TierTable[i].MaxScore)
                return i;
        }
        return 0;
    }
    public int CalcFinalTierScore(List<int> myRanksPerRound) {
        // myRanksPerRound: [1R 등수, 2R 등수, 3R 등수]
        // 예: [1, 3, -1] (3라운드 못 간 경우 -1 등)
        int totalScore = 0;

        // 1라운드
        int r1 = myRanksPerRound.Count > 0 ? myRanksPerRound[0] : -1;
        int r2 = myRanksPerRound.Count > 1 ? myRanksPerRound[1] : -1;
        int r3 = myRanksPerRound.Count > 2 ? myRanksPerRound[2] : -1;

        // 1R
        if (r1 != -1)
            totalScore += CalcRoundScore(1, r1);

        // 2R (탈락이면 보정점수, 진출이면 원래 점수)
        if (r2 != -1) {
            int basic = CalcRoundScore(2, r2);

            // 3,4등 = 탈락자만 보정
            if (r2 >= 3 && r2 <= 4) {
                if (r1 == 1) basic = -2;
                else if (r1 == 2) basic = -4;
                // 3,4등은 기본(-5, -10)
            }
            totalScore += basic;
        }

        // 3R
        if (r3 != -1)
            totalScore += CalcRoundScore(3, r3);

        if (totalScore < 0)
            totalScore = 0;

        return totalScore;
    }

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

public class TierInfo
{
    public int Index;
    public string Name;
    public int MinScore;
    public int MaxScore;
    // public string IconPath; // 

    public TierInfo(int index, string name, int min, int max) {
        Index = index;
        Name = name;
        MinScore = min;
        MaxScore = max;
        // IconPath = ... // 
    }
}
