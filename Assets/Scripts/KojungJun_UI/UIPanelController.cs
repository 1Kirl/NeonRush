using UnityEngine;
using DG.Tweening;
using System.Collections;

[System.Serializable]
public class UIPanelController
{
    #region Serialized Variables

    [SerializeField] private GameObject _panelObject;
    [SerializeField] private CanvasGroup _canvasGroup;

    #endregion




    #region public Funcs

    public void InstantlyHide() {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _panelObject.SetActive(false);
    }

    public void ShowInstantly() {
        _panelObject.SetActive(true);
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    #endregion
}
