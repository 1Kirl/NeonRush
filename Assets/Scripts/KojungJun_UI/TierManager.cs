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
    new TierInfo(0, "Bronze",      0,   499),
    new TierInfo(1, "Silver",    500,   999),
    new TierInfo(2, "Gold",     1000,  1499),
    new TierInfo(3, "Platinum", 1500,  1999),
    new TierInfo(4, "Diamond",  2000,  2499),
    new TierInfo(5, "Challenger", 2500, int.MaxValue)
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
    

    /// <summary>
    /// 라운드/등수에 따른 점수 계산 (보정 미포함)
    /// </summary>

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
