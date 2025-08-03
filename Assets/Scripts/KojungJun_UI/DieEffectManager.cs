using UnityEngine;
using static ShopItemButton;
using ClientInfo;
public class DieEffectManager : BaseItemManager<int>
{
    public static DieEffectManager Instance;

    protected override string UnlockedColumnName => "unlockedDieEffectIds";
    protected override string EquippedColumnName => "equippedDieEffectIds";

    protected override int ParseItemId(string raw) => int.Parse(raw);
    protected override int GetDefaultItemId() => 0;

    protected override ShopItemType GetItemType() => ShopItemType.DieEffect; 

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
    }

    private void Start() {
        LoadFromBackend();
    }
}
