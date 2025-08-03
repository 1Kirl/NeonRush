using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using ClientInfo;
using BackEnd;
using System.Collections.Generic;

public class ShopItemButton : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text labelText;
    public Image diamondImage;
    public Button purchaseButton;
    public RectTransform targetTransform;

    [Header("Data")]
    public ShopItemType itemType;
    public int itemId;
    public int price;
    [SerializeField] private bool isLocked = false;

    [Header("Lock Visuals")]
    [SerializeField] private GameObject _lockIcon;
    [SerializeField] private Image _productImage;
    [SerializeField] private Image _priceImage;
    [SerializeField] private bool isDummy = false;

    [SerializeField] private RectTransform priceImageRect;
    [SerializeField] private RectTransform priceTextRect;

    private bool isPurchased = false;
    private bool isEquipped = false;
    private bool wasLocked = false;

    //public enum ShopItemType { DieEffect, Trail, CarKind }

    public void Unlock() {
        isLocked = false;
        RefreshVisualState();
    }

    private void Awake() {
        if (targetTransform == null)
            targetTransform = GetComponent<RectTransform>();

        if (labelText == null)
            Debug.LogWarning($"[ShopItemButton] {gameObject.name} �� labelText is not assigned.");
    }

    private void Start() {
        purchaseButton?.onClick.AddListener(OnItemClick);
        targetTransform.localScale = Vector3.one * 1.5f;

        if (!isDummy) {
            GetManagerByType().OnEquipChanged += HandleEquipChanged;
        }

        RefreshVisualState();
    }

    private void OnItemClick() {
        if (isDummy || isLocked) {
            PurchasePopup.Instance.Show("Locked Product!");
            return;
        }

        if (!isPurchased) {
            PurchasePopup.Instance.Show($"Do you want to buy this item for {price} coins?", () => {
                PurchaseItem();
            });
        }
        else {
            EquipItem();
        }
    }

    private void PurchaseItem() {
        var manager = GetManagerByType();

        manager.BuyItem(itemId, price, () => CurrencyManager.Instance.TrySpendCoins(price), () => {
            isPurchased = true;

            manager.EquipItem(itemId, () => {
                isEquipped = true;
                SetLabel("Equipped");
                AnimateScale(Vector3.one * 1.6f);
            });

            SetDiamondVisible(false);
            CurrencyManager.Instance.SaveCoinsToBackend();

            // UI 반영
            MainThreadDispatcher.Enqueue(() => {
                FindObjectOfType<CoinUI>()?.RefreshCoinUI();
            });
        });
    }



    private void EquipItem() {
        var manager = GetManagerByType();
        manager.EquipItem(itemId, () => {
            isEquipped = true;
            SetLabel("Equipped");
            AnimateScale(Vector3.one * 1.6f);
        });
    }

    private void HandleEquipChanged(int equippedId) {
        if (isLocked || isDummy) return;

        var manager = GetManagerByType();
        if (!manager.IsUnlocked(itemId)) return;

        isEquipped = itemId == equippedId;

        SetLabel(isEquipped ? "Equipped" : "Equip");
        AnimateScale(isEquipped ? Vector3.one * 1.6f : Vector3.one * 1.5f);
    }

    private void RefreshVisualState() {
        if (isDummy) {
            SetLabel("??");
            SetDiamondVisible(false);
            if (_lockIcon != null) _lockIcon.SetActive(true);
            if (_productImage != null) _productImage.gameObject.SetActive(false);
            if (_priceImage != null) _priceImage.color = Color.gray;
            wasLocked = isLocked;
            return;
        }

        var manager = GetManagerByType();
        isPurchased = manager.IsUnlocked(itemId);
        isEquipped = manager.IsEquipped(itemId);

        if (_lockIcon != null) _lockIcon.SetActive(isLocked);
        if (_productImage != null) _productImage.gameObject.SetActive(!isLocked);
        if (_priceImage != null) _priceImage.color = isLocked ? Color.gray : Color.green;

        // ��ġ ���� - Dummy ���� + price ���� + rect ���� Ȯ��
        if (price >= 1000 && priceImageRect != null && priceTextRect != null) {
            if (isLocked) {
                priceImageRect.anchoredPosition = new Vector2(22, -72);
                priceTextRect.anchoredPosition = new Vector2(33, -72);
            }
            else if (!isLocked && wasLocked) {
                priceImageRect.anchoredPosition = new Vector2(38, -72);
                priceTextRect.anchoredPosition = new Vector2(51, -72);
            }
        }

        if (isLocked) {
            SetLabel("??");
            SetDiamondVisible(true);
        }
        else if (!isPurchased) {
            SetLabel(price.ToString());
            SetDiamondVisible(true);
        }
        else {
            SetLabel(isEquipped ? "Equipped" : "Equip");
            SetDiamondVisible(false);
        }

        AnimateScale(isEquipped ? Vector3.one * 1.6f : Vector3.one * 1.5f);

        wasLocked = isLocked;
    }

    private void SetLabel(string text) {
        if (labelText == null) return;

        labelText.text = text;
        labelText.enableAutoSizing = false;

        if (text == "Equipped") {
            labelText.fontSize = 20;
            labelText.alignment = TextAlignmentOptions.Top;
            labelText.color = Color.green;
            labelText.rectTransform.anchoredPosition = new Vector2(77, -77);
        }
        else if (text == "Equip") {
            labelText.fontSize = 20;
            labelText.alignment = TextAlignmentOptions.Top;
            labelText.color = Color.white;
            labelText.rectTransform.anchoredPosition = new Vector2(77, -77);
        }
        else {
            labelText.fontSize = 30;
        }
    }

    private void SetDiamondVisible(bool visible) {
        if (diamondImage != null)
            diamondImage.gameObject.SetActive(visible);
    }

    private void AnimateScale(Vector3 target) {
        if (targetTransform == null) return;
        targetTransform.DOKill();
        targetTransform.DOScale(target, 0.25f).SetEase(Ease.OutBack);
    }

    private BaseItemManager<int> GetManagerByType() {
        return itemType switch {
            ShopItemType.DieEffect => DieEffectManager.Instance,
            ShopItemType.Trail => TrailManager.Instance,
            _ => throw new Exception("Unsupported item type")
        };
    }
}
