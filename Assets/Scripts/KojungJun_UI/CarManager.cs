using static ShopItemButton;
using ClientInfo;
public class CarManager : BaseItemManager<int>
{
    public static CarManager Instance;

    protected override string UnlockedColumnName => "unlockedCarIds";
    protected override string EquippedColumnName => "equippedCarId";

    protected override int ParseItemId(string raw) => int.Parse(raw);
    protected override int GetDefaultItemId() => 0;

    protected override ShopItemType GetItemType() => ShopItemType.CarKind; 

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start() {
        LoadFromBackend();
    }
}
