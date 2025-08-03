using BackEnd;
using System.Collections.Generic;
using System;
using UnityEngine;
using LitJson;
using static ShopItemButton;
using ClientInfo;

public abstract class BaseItemManager<T> : MonoBehaviour
{
    public bool IsInitialized { get; protected set; } = false;

    protected List<T> _unlockedItems = new();
    protected T _equippedItem;

    public T EquippedItem => _equippedItem;

    public bool IsUnlocked(T id) => _unlockedItems.Contains(id);
    public bool IsEquipped(T id) => EqualityComparer<T>.Default.Equals(_equippedItem, id);

    protected abstract string UnlockedColumnName { get; }
    protected abstract string EquippedColumnName { get; }

    public event Action OnDataLoaded;
    public event Action<T> OnEquipChanged;
    protected abstract ShopItemType GetItemType();
    public static event Action<ShopItemType, int> OnItemEquippedGlobal; 


    public void LoadFromBackend() {
        Backend.GameData.GetMyData("user_data", new Where(), callback => {
            if (!callback.IsSuccess())
            {
                return;
            }

            var row = callback.FlattenRows()[0];

            if (row.ContainsKey(UnlockedColumnName)) {
                JsonData listData = row[UnlockedColumnName];
                _unlockedItems.Clear();

                for (int i = 0; i < listData.Count; i++) {
                    try {
                        T value = ParseItemId(listData[i].ToString());
                        _unlockedItems.Add(value);
                    }
                    catch (Exception e) {
                        Debug.LogWarning($"[{GetType().Name}] {UnlockedColumnName}[{i}]: {e.Message}");
                    }
                }
            }

            if (row.ContainsKey(EquippedColumnName)) {
                _equippedItem = ParseItemId(row[EquippedColumnName].ToString());
            }

            Debug.Log($"[{GetType().Name}] Unlocked: {string.Join(", ", _unlockedItems)}, Equipped: {_equippedItem}");

            MainThreadDispatcher.Enqueue(() => {
                OnDataLoaded?.Invoke();
                IsInitialized = true;
            });
        });

        EnsureDefaultItem();
    }

    public void BuyItem(T id, int price, Func<bool> spendCurrency, Action onSuccess = null) {
        if (IsUnlocked(id)) {
            return;
        }

        if (!spendCurrency()) {
            MainThreadDispatcher.Enqueue(() =>
            {
                PurchasePopup.Instance.Show("Not enough Coins.");
                SoundManager.Instance.PlaySFX(SFXType.Popup);  // coin not enough
            });
            return;
        }

        CurrencyManager.Instance.SaveCoinsToBackend();
        _unlockedItems.Add(id);

        SoundManager.Instance.PlaySFX(SFXType.Buy); // Purchase Item
        UpdateBackend(onSuccess);
    }

    public void EquipItem(T id, Action onSuccess = null) {
        if (EqualityComparer<T>.Default.Equals(_equippedItem, id)) {
            return;
        }

        _equippedItem = id;
        UpdateBackend(() => {
            MainThreadDispatcher.Enqueue(() => {
                OnEquipChanged?.Invoke(id);

                if (int.TryParse(id.ToString(), out int intId))
                    OnItemEquippedGlobal?.Invoke(GetItemType(), intId);

                SoundManager.Instance.PlaySFX(SFXType.Equip);  // Equip Item
                onSuccess?.Invoke();
            });
        });
    }



    private void UpdateBackend(Action onComplete = null) {
        Param param = new Param
        {
            { UnlockedColumnName, _unlockedItems },
            { EquippedColumnName, _equippedItem.ToString() }
        };

        Backend.GameData.Update("user_data", new Where(), param, callback => {
            if (onComplete != null) {
                MainThreadDispatcher.Enqueue(onComplete);
            }
        });
    }

    private void EnsureDefaultItem() {
        T defaultId = GetDefaultItemId();
        if (!_unlockedItems.Contains(defaultId)) {
            _unlockedItems.Add(defaultId);
        }
    }

    protected abstract T ParseItemId(string raw);
    protected virtual T GetDefaultItemId() => default;
}
