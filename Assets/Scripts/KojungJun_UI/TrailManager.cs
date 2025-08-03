using static ShopItemButton;
using ClientInfo;
public class TrailManager : BaseItemManager<int>
{
    public static TrailManager Instance;

    protected override string UnlockedColumnName => "unlockedTrail";
    protected override string EquippedColumnName => "equippedTrail";

    protected override int ParseItemId(string raw) => int.Parse(raw);
    protected override int GetDefaultItemId() => 0;

    protected override ShopItemType GetItemType() => ShopItemType.Trail;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        LoadFromBackend();
    }
}
