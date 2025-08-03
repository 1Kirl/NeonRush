using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    #region Fields

    [SerializeField] private Image _itemImage;
    [SerializeField] private Text _itemNameText;
    [SerializeField] private Text _priceText;

    private ShopItemData _data;

    #endregion





    public void Setup(ShopItemData data) {
        _data = data;
        _itemImage.sprite = data.itemImage;
        _itemNameText.text = data.itemName;
        _priceText.text = data.price.ToString("N0");
    }

    public void OnBuy() {
        Debug.Log($"{_data.itemName} 备概 矫档!");
        // TODO: 角力 备概 贸府
    }
}
