using UnityEngine;
using Cinemachine;
using System.Collections;

public class ShopCameraController : MonoBehaviour
{
    #region Serialized Variables

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera shopCamera;

    [Header("Shop Targets")]
    [SerializeField] private Transform[] shopTargets;

    [SerializeField] private CarShopManager carShopManager;

    #endregion

    #region Private Variables

    private int _currentIndex = 0;
    private Coroutine rotationCoroutine;
    private CarRotator currentRotator;

    #endregion

    #region Public Functions

    public void PressNext() {
        StopCurrentRotation();

        _currentIndex = (_currentIndex + 1) % shopTargets.Length;
        ApplyCameraTarget();
        carShopManager.ShowNextCar();
        StartCarRotation();
        UIManager.Instance.FlashPrevButton();
    }

    public void PressPrevious() {
        StopCurrentRotation();

        _currentIndex = (_currentIndex - 1 + shopTargets.Length) % shopTargets.Length;
        ApplyCameraTarget();
        carShopManager.ShowPreviousCar();
        StartCarRotation();
        UIManager.Instance.FlashNextButton();
    }

    public void ResetCamera() {
        ApplyCameraTarget();
    }

    public void SetCameraIndex(int index) {
        StopCurrentRotation();

        _currentIndex = index;
        ApplyCameraTarget();
        StartCarRotation();

        Debug.Log($"[ShopCam] SetCameraIndex ȣ��: Index {_currentIndex}");
    }

    public void SetCameraToEquippedInstantly() {
        int index = carShopManager.GetIndexOfEquippedCar();
        if (index >= 0 && index < shopTargets.Length) {
            StopCurrentRotation();

            _currentIndex = index;
            shopCamera.OnTargetObjectWarped(shopCamera.Follow, shopTargets[index].position);
            ApplyCameraTarget();
            carShopManager.ForceSetIndex(index);
            StartCarRotation();

            Debug.Log($"[ShopCam] �ν���Ʈ ī�޶� ���� �Ϸ�: Index {_currentIndex}");
        }
    }

    public void EnterCollectionPanel() {
        Debug.Log("[EnterCollectionPanel] ȣ���");
        SetCameraToEquippedInstantly();
        StartCarRotation();
    }


    public void ExitCollectionPanel() {
        StopCurrentRotation();
    }

    #endregion

    #region Private Functions

    private void ApplyCameraTarget() {
        var target = shopTargets[_currentIndex];
        shopCamera.Follow = target;
        shopCamera.LookAt = target;
    }

    private void StartCarRotation() {
        StopCurrentRotation(); 

        GameObject bodyObject = carShopManager.GetBodyObjectAt(_currentIndex);
        if (bodyObject == null) {
            Debug.LogWarning("[StartCarRotation] bodyObject is null at index: " + _currentIndex);
            return;
        }

        currentRotator = bodyObject.GetComponent<CarRotator>();
        if (currentRotator != null) 
            rotationCoroutine = StartCoroutine(RotateTargetCar(currentRotator));
        
        carShopManager.ResetAllOtherCarRotations(_currentIndex);
    }


    private void StopCurrentRotation() {
        if (rotationCoroutine != null) {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }

        if (currentRotator != null) {
            currentRotator.ResetRotation();
            currentRotator = null;
        }
    }


    private IEnumerator RotateTargetCar(CarRotator rotator) {
        while (true)
        {
            rotator.RotateOnce();
            yield return null;
        }
    }

    #endregion
}
