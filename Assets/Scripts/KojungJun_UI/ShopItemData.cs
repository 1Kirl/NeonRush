using UnityEngine;



#region Definitions

[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Item")]
public class ShopItemData : ScriptableObject
{
    #region Public Variables

    public string itemId;
    public string itemName;
    public Sprite itemImage;
    public int price;
    public bool isPremium;
    public GameObject previewModel;

    #endregion
}

#endregion
