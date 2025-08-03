using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class PurchasePopup : MonoBehaviour
{
    public static PurchasePopup Instance;

    public GameObject popupRoot;
    public GameObject popupRoot_NotEnoughCoin;
    public TMP_Text messageText;
    public TMP_Text warningText_NotEnoughCoin;
    public Button okButton;
    public Button okButton_NotEnoughCoin;
    public Button cancelButton;

    private Action _onConfirm;

    private void Awake() {
        Instance = this;
        popupRoot.SetActive(false);
        popupRoot_NotEnoughCoin.SetActive(false); 

        okButton.onClick.AddListener(() => {
            popupRoot.SetActive(false);
            _onConfirm?.Invoke();
        });

        cancelButton.onClick.AddListener(() => {
            popupRoot.SetActive(false);
        });

        okButton_NotEnoughCoin.onClick.AddListener(() => {
            popupRoot_NotEnoughCoin.SetActive(false);
        });
    }

    public void Show(string message, Action onConfirm) {
        popupRoot.SetActive(true);
        messageText.text = message;
        _onConfirm = onConfirm;
    }

    public void Show(string message) // NotEnough
    {
        popupRoot_NotEnoughCoin.SetActive(true);
        warningText_NotEnoughCoin.text = message;
        _onConfirm = null;
    }
}
