using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    #region public valuables
    public CinemachineVirtualCamera normalCamera;
    public CinemachineVirtualCamera fastCamera;
    public CinemachineVirtualCamera flipCamera;
    public CinemachineVirtualCamera carStareCamera;
    public CinemachineVirtualCamera deathCamera;
    public GameObject StartDollyCart;
    public Transform StartDollyTrack;
    public Transform StartCylinder;
    public Transform EndDollyTrack;
    public GameObject EndDollyCart;
    public GameObject car;
    public WheelCollider[] wheels;
    public float speedThreshold = 50f; // threshold of switching to fastCam

    #endregion





    #region private valuables
    private Rigidbody carRigidbody;
    private CinemachineVirtualCamera lastActiveCamera; // Save current Cam
    private bool isFastCamActive = false;
    private bool isFlipCamActive = false;
    private bool isGameStarted = false;
    #endregion






    #region SerializeField private valuables
    [SerializeField] private Car1Controller car1Controller;
    [SerializeField] private float currentSpeed;
    [SerializeField] private bool allWheelsOffGround = false;
    #endregion

    void Start()
    {
        if (car != null)
        {
            carRigidbody = car.GetComponent<Rigidbody>();
        }
        if (car1Controller != null)
        {
            car1Controller.OnSpeedMetThreshold += SwitchToFastCamera;
            car1Controller.OnSpeedGotLowThreshold += SwitchToNormalCamera;
        }
        // Initial Camera Setting
        normalCamera.Priority = 2;
        fastCamera.Priority = 1;
        flipCamera.Priority = 0; // basically hiden
    }

    void Update()
    {
        if (carRigidbody == null || wheels[0] == null || isGameStarted == false) return; // check wheather essensial components are adjusted

        currentSpeed = carRigidbody.velocity.magnitude;
        allWheelsOffGround = AreAllWheelsOffGround();

        // if all wheels are off the ground, activate filpCam immediately
        if (allWheelsOffGround && !isFlipCamActive)
        {
            // it is okay about the worry of what if it looked akward switching the cam to flipCam immediately
            // because the switching between cameras will proceed gradually in time
            SwitchToFlipCamera();
            return;
        }

        // return to the original camera when all the wheels touch the ground again
        if (!allWheelsOffGround && isFlipCamActive)
        {
            RestoreLastCamera();
            return;
        }

    }
    // logic for switching between fast cam and normal cam
    private void SwitchToFastCamera()
    {
        normalCamera.Priority = 1;
        fastCamera.Priority = 2;
        isFastCamActive = true;
        Debug.Log("[CM] switch to fast cam");
    }
    private void SwitchToNormalCamera()
    {
        normalCamera.Priority = 2;
        fastCamera.Priority = 1;
        isFastCamActive = false;
        Debug.Log("[CM] switch to normal cam");
    }
    public void GameStart()
    {
        Debug.Log("[CM] GameStartButton");
        isGameStarted = true;
        //carStareCamera.Follow = car.transform;

        deathCamera.Priority = 0;
        carStareCamera.Priority = 0;
    }

    void SwitchToFlipCamera()
    {
        lastActiveCamera = isFastCamActive ? fastCamera : normalCamera; // store the originally active camera
        flipCamera.Priority = 3; // filpCam gets the highest priority
        isFlipCamActive = true;
    }
    public void BackToMenu()
    {
        carStareCamera.Priority = 3;
    }

    void RestoreLastCamera()
    {
        flipCamera.Priority = 0; // deactivate the filpCam
        // the camera with priority 2 is displayed on the screen
        isFlipCamActive = false;
    }

    public void ResetToStart()
    {
        Debug.Log("[CM] ResetToStart");
        deathCamera.Priority = 0;
        carStareCamera.Priority = 4;
    }
    public void SwitchToDeathCam()
    {
        Debug.Log("[CM] SwitchToDeathCam");
        deathCamera.Priority = 4;
    }
    public void FinishCamMove(Transform carTransform)
    {
        EndDollyTrack.position = carTransform.position;
        EndDollyTrack.rotation = carTransform.rotation;
        EndDollyCart.SetActive(true);
        carStareCamera.Priority = 5;
        carStareCamera.Follow = EndDollyCart.transform;
    }
    public void StareCamOff()
    {
        carStareCamera.Priority = 0;
    }
    bool AreAllWheelsOffGround()
    {
        foreach (WheelCollider wheel in wheels)
        {
            if (wheel.isGrounded) return false;
        }
        return true; // all wheels are off the ground
    }
    public void BindPlayerToCams(GameObject myCar, bool isMulti)
    {
        normalCamera.Follow = myCar.transform;
        normalCamera.LookAt = myCar.transform;
        deathCamera.Follow = myCar.transform;
        deathCamera.LookAt = myCar.transform;
        fastCamera.Follow = myCar.transform;
        fastCamera.LookAt = myCar.transform;
        FilpTargetController filpTargetController = GameObject.Find("FlipTarget").GetComponent<FilpTargetController>();
        filpTargetController.Car = myCar;
        if (isMulti)
        {
            StartDollyTrack.position = myCar.transform.position;
            StartDollyTrack.rotation = myCar.transform.rotation;
            carStareCamera.Follow = StartDollyCart.transform;
            carStareCamera.LookAt = myCar.transform;
        }
        else
        {
            carStareCamera.Follow = StartCylinder.transform;
            carStareCamera.LookAt = StartCylinder.transform;
        }

        //carStareCamera.LookAt = myCar.transform;
        FogController fogController = GameObject.Find("Fog").GetComponent<FogController>();
        fogController.car = myCar;
    }
    public void BindWheels(GameObject myCar)
    {
        Debug.Log("[Camera Manager] Initialize Wheels to CamManager");
        wheels = myCar.GetComponentsInChildren<WheelCollider>();
    }
    public void InitializeCarAndCamManager(GameObject myCar, bool isMulti)
    {
        Debug.Log("[Camera Manager] Initialize Car and CamManager");
        //Bind Wheels
        BindWheels(myCar);
        //Bind CarController
        car = myCar;
        car1Controller = car.GetComponent<Car1Controller>();
        carRigidbody = car.GetComponent<Rigidbody>();
        car1Controller.OnSpeedMetThreshold += SwitchToFastCamera;
        car1Controller.OnSpeedGotLowThreshold += SwitchToNormalCamera;

        // Initial Camera Setting
        carStareCamera.Priority = 4;
        normalCamera.Priority = 2;
        fastCamera.Priority = 1;
        flipCamera.Priority = 0; // basically hiden
        BindPlayerToCams(myCar, isMulti);
    }
}
