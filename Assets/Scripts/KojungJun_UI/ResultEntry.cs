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

    public ResultEntry(ushort clientId, string name, ushort baseScore, byte arrivalRank) {
        ClientId = clientId;
        Name = name;
        BaseScore = baseScore;
        ArrivalRank = arrivalRank;
        BonusScore = baseScore; // Default before bonus applied
    }

    /// <summary>
    /// Applies ranking bonus based on arrival order.
    /// </summary>
    public void ApplyRankingBonus() {
        float multiplier = ArrivalRank switch {
            1 => 2f,
            2 => 1.5f,
            3 => 1.25f,
            _ => 1f
        };

        BonusScore = (ushort)(BaseScore * multiplier);
    }

    public override string ToString() {
        return $"{Name} (ID: {ClientId}) | Base: {BaseScore}, Bonus: {BonusScore}, Rank: {(HasReachedFinish ? ArrivalRank.ToString() : "N/A")}";
    }
}
