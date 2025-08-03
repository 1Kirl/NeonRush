using UnityEngine;



public class ShopPreviewManager : MonoBehaviour
{

    #region Public variables

    public static ShopPreviewManager Instance;

    #endregion


    #region Serialized Variables

    [SerializeField] private Transform _spawnPoint;

    #endregion





    #region Private Variables

    private GameObject _currentModel;

    #endregion





    #region Unity Functions

    private void Awake() {
        Instance = this;
    }

    #endregion
     




    #region Public Functions

    public void ShowPreview(ShopItemData data) {
        if (_currentModel != null) {
            Destroy(_currentModel);
        }

        _currentModel = Instantiate(data.previewModel, _spawnPoint.position, Quaternion.identity);
    }

    #endregion
}
