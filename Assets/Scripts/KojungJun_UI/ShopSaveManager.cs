using UnityEngine;
using System.Collections.Generic;



public static class ShopSaveManager
{
    #region Defination

    private const string Prefix = "SHOP_UNLOCKED_";
    private const string SelectedKey = "SELECTED_ITEM";

    #endregion





    #region Public Functions

    public static bool IsUnlocked(string itemId) {
        return PlayerPrefs.GetInt(Prefix + itemId, 0) == 1;
    }


    public static void UnlockItem(string itemId) {
        PlayerPrefs.SetInt(Prefix + itemId, 1);
    }


    public static void ResetAll() {
        PlayerPrefs.DeleteAll();
    }

    public static void SetSelected(string itemId) {
        PlayerPrefs.SetString(SelectedKey, itemId);
    }

    public static string GetSelected() {
        return PlayerPrefs.GetString(SelectedKey, string.Empty);
    }

    #endregion
}
