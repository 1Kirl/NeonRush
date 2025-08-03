public class RankingEntry
{
    public ushort ClientId;
    public ushort Score;
    public string Name;

    public RankingEntry(ushort id, string name, ushort score) {
        ClientId = id;
        Name = name;
        Score = score;
    }
}