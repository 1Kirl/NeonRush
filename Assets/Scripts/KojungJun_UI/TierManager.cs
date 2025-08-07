using System;
using System.Collections.Generic;

public class TierManager
{
    // int: 0~20  ºê4~Ã§¸°Àú
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
