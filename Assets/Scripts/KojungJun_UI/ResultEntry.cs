using System;

public class ResultEntry
{
    public ushort ClientId { get; }
    public string Name { get; }

    // Base score collected during gameplay
    public ushort BaseScore { get; }

    // Bonus score calculated based on arrival rank
    public ushort BonusScore { get; private set; }

    // Arrival rank at finish line (0 if not reached)
    public byte ArrivalRank { get; }

    public bool HasReachedFinish => ArrivalRank > 0;

    public ResultEntry(ushort clientId, string name, ushort baseScore, ushort bonusScore ,byte arrivalRank)
    {
        ClientId = clientId;
        Name = name;
        BaseScore = baseScore;
        ArrivalRank = arrivalRank;
        BonusScore = bonusScore; // 서버에서 미리 계산해서 줌
    }
    public override string ToString() {
        return $"{Name} (ID: {ClientId}) | Base: {BaseScore}, Bonus: {BonusScore}, Rank: {(HasReachedFinish ? ArrivalRank.ToString() : "N/A")}";
    }
}
