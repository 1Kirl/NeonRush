using UnityEngine;

public class UIButton : MonoBehaviour
{
    #region Fields

    [SerializeField] private PanelType panelType;

    #endregion




    #region UI Button Click
    public void OnClick() {
        UIManager.Instance.OpenPanel(panelType);
        UIManager.Instance.SetAchievementButtonVisible(panelType != PanelType.Collection);
    }


    #endregion
}

