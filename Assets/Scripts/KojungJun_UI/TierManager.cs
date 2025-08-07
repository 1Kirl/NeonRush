using System;
using System.Collections.Generic;

/// <summary>
/// Ƽ�� ���/����/��Ī ���� (�̱���, ���� X)
/// </summary>
public class TierManager
{
    // �̱��� �ν��Ͻ�
    private static TierManager _instance;
    public static TierManager Instance => _instance ??= new TierManager();

    // Ƽ�� ���̺�(����: �ε���/��Ī/��������)
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
    /// ���� �� Ƽ�� ��ü ��ȯ
    /// </summary>
    public TierInfo GetTierInfo(int tierScore) {
        foreach (var t in TierTable) {
            if (tierScore >= t.MinScore && tierScore <= t.MaxScore)
                return t;
        }
        // score < 0 ����
        return TierTable[0];
    }

    /// <summary>
    /// �ε��� �� Ƽ�� ��ü ��ȯ
    /// </summary>
    public TierInfo GetTierInfoByIndex(int tierIndex) {
        if (tierIndex < 0) return TierTable[0];
        if (tierIndex >= TierTable.Count) return TierTable[TierTable.Count - 1];
        return TierTable[tierIndex];
    }

    /// <summary>
    /// ���� �� Ƽ�� �ε��� ��ȯ
    /// </summary>
    public static int GetTierIndexByScore(int tierScore) {
        for (int i = 0; i < TierTable.Count; i++) {
            if (tierScore >= TierTable[i].MinScore && tierScore <= TierTable[i].MaxScore)
                return i;
        }
        return 0;
    }

    /// <summary>
    /// ���庰 ��� ����Ʈ �� ���� ���� Ƽ������ ���(�������� ����)
    /// </summary>
    

    /// <summary>
    /// ����/����� ���� ���� ��� (���� ������)
    /// </summary>

}

/// <summary>
/// �� Ƽ��(���) ����
/// </summary>
public class TierInfo
{
    public int Index;
    public string Name;
    public int MinScore;
    public int MaxScore;
    // public string IconPath; // �ʿ�� Ȯ��

    public TierInfo(int index, string name, int min, int max) {
        Index = index;
        Name = name;
        MinScore = min;
        MaxScore = max;
    }
}
