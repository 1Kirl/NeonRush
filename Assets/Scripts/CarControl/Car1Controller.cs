using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Car1Controller : MonoBehaviour
{

    private const int LANDING_BOOST = 0;
    private const int DRIFT_BOOST = 1;

    #region public variables

    [Header("Spin Pivots")]
    public Transform SpinPointL;
    public Transform SpinPointR;

    #endregion





    #region SerializeFied private variables

    [Header("Car Power Setting")]
    [SerializeField] private PlayerInput playerInput;

    [Tooltip("The force of acceleration")]
    [SerializeField] private float motorTorque;

    [Tooltip("The force of brake.")]
    [SerializeField] private float brakeTorque;

    [Tooltip("The maximum angle at which the steering wheel can be turned.")]
    [SerializeField] private float maxSteerAngle;
    [SerializeField] private float maxSteerAngleWithDrift;

    [Tooltip("The force exerted on the car when performing a flip.")]
    [SerializeField] private float flipForce;

    // 수정사항들에 쓰이는 애들
    [Tooltip("The force exerted on the car when performing a spin.")]
    [SerializeField] private float spinForce;
    //[SerializeField] private float sameDirectionSpinForce;
    //[SerializeField] private float oppositeDirectionSpinForce;
    [SerializeField] private float yAngularSpeedThreshold;
    [SerializeField] private float zAngularSpeedThreshold;
    [SerializeField] private float rotationLerpSpeedForZ = 5f;
    private bool isStoppingRotationForZ = false;
    [SerializeField] private float flipAngularSpeed;
    [SerializeField] private float spinAngularSpeed;
    [SerializeField] private float smoothingFactorX;
    [SerializeField] private float smoothingFactorY;
    private bool isFlipping = false;
    private float airTimeForFlip = 0f; // 공중에 떠 있는 시간 추적
    private float airTimeForSpin = 0f;
    private float airTimeForZ = 0f;
    public float flipAndSpinDelay = 0.5f; // 플립 가능까지 대기 시간 (예: 0.5초)
    private bool isSpinning = false;
    private bool isZFinish = false;
    [SerializeField] private float driftDurationOnSpeedPad = 1.3f; // SpeedPad 충돌 시 드리프트 지속 시간
    private bool isDriftFromSpeedPad = false; // SpeedPad로 인한 드리프트 상태
    private float driftTimer = 0f; // 드리프트 지속 시간 추적
    [SerializeField] private float maxSelfSpeed = 30f; // 자동차 자체 힘으로 낼 수 있는 최대 속도
    private Coroutine _frictionRecoveryCoroutine;
    [SerializeField] private float driftRecoverTime = 1.5f;


    [Tooltip("Power of Boost at laning")]
    [SerializeField] private float landingThresholdTime;


    [Header("Car Start Position")]
    [SerializeField] public Transform StartPoint;
    [SerializeField] public GameObject StartCylinder;


    [Header("WheelColliders: [0],[1]: Front / [2],[3]: Rear")]
    // Wheel colliders. physically interact with the environment.
    [SerializeField] private WheelCollider[] wheelColliders = new WheelCollider[4];



    [Header("🟩 Default (Normal Mode) Friction Settings")]
    [SerializeField] private float defaultForwardExtremumSlip = 0.4f;
    [SerializeField] private float defaultForwardExtremumValue = 1f;
    [SerializeField] private float defaultForwardAsymptoteSlip = 0.8f;
    [SerializeField] private float defaultForwardAsymptoteValue = 0.5f;
    [SerializeField] private float defaultForwardStiffness = 2.0f;

    [SerializeField] private float defaultSideExtremumSlip = 0.2f;
    [SerializeField] private float defaultSideExtremumValue = 1f;
    [SerializeField] private float defaultSideAsymptoteSlip = 0.5f;
    [SerializeField] private float defaultSideAsymptoteValue = 0.75f;
    [SerializeField] private float defaultSideStiffness = 2.0f;

    [Header("🟥 Drift Mode Friction Settings")]
    [SerializeField] private float driftForwardExtremumSlip = 0.3f;
    [SerializeField] private float driftForwardExtremumValue = 0.8f;
    [SerializeField] private float driftForwardAsymptoteSlip = 0.5f;
    [SerializeField] private float driftForwardAsymptoteValue = 0.4f;
    [SerializeField] private float driftForwardStiffness = 1.5f;

    [SerializeField] private float driftSideExtremumSlip = 0.1f;
    [SerializeField] private float driftSideExtremumValue = 0.6f;
    [SerializeField] private float driftSideAsymptoteSlip = 0.2f;
    [SerializeField] private float driftSideAsymptoteValue = 0.3f;
    [SerializeField] private float driftSideStiffness = 1.2f;


    [Header("Visual Wheels")]
    // Wheels. A visual role without physical interaction.
    [SerializeField] private Transform[] wheelTransforms = new Transform[4];



    [Header("Boost")]

    [Tooltip("Power of Boost at laning")]
    [SerializeField] private float landingBoostSpeed;
    [SerializeField] private float driftBoostSpeed;
    [SerializeField] private float boostTimeOfLanding;
    [SerializeField] private float boostTimeOfDrift;

    [Header("Drift")]
    [SerializeField] private float timeThresholdToDriftBoost;



    [Header("Speed")]
    [SerializeField] private float _currentSpeed;
    [SerializeField] private float speedThreshold = 50f;
    //[SerializeField] private float driftOffspeedThreshold = 50f;

    [Header("Gravity")]
    [SerializeField] private float additionalGravity;
    #endregion





    #region Private variables

    private Local_InputReceiver local_InputReceiver;
    private Rigidbody carRB;

    private float _horizontalInput;
    private float _verticalInput;
    private float _currentsteerAngle;
    private float _currentBrakeForce;
    private bool _isBoosting = false;
    private bool _wasGrounded;
    private float _landingTime;
    private bool _okToDriftBoost = false;
    private bool _isFast = false;
    private bool _driftKeyPressed = false;
    private bool _isDrift = false;
    private float _driftTime = 0.0f;
    private bool _driftPause = false;

    [SerializeField]public bool isLocal = true;

    #endregion





    #region properties
    public bool OkToDriftBoost
    {
        get => _okToDriftBoost;
        set
        {
            _okToDriftBoost = value;
        }
    }
    public bool IsDrift
    {
        get => _isDrift;
        set
        {
            if (_isDrift != value && value == true)
            {
                // Invoke Charging Effect **ONCE**
                OnDriftCharging.Invoke();
            }
            _isDrift = value;
            OnDriftSkidMark?.Invoke(value);
        }
    }
    public float CurrentSpeed
    {
        get => _currentSpeed;
        set
        {
            _currentSpeed = value;
            if (value > speedThreshold && !_isFast)
            {
                OnSpeedMetThreshold?.Invoke();
                _isFast = true;
                Debug.Log("switch to normal cam - car");
                /*
                Debug.Log("current speed: " + _currentSpeed);
                Debug.Log("value: " + value);
                Debug.Log("speedThreshold: " + speedThreshold);
                */
            }
            else if (value <= speedThreshold && _isFast)
            {
                OnSpeedGotLowThreshold?.Invoke();
                _isFast = false;
                Debug.Log("switch to normal cam - car");
            }

            if (_currentSpeed > 20 && _driftKeyPressed && _horizontalInput != 0 && !IsDrift && AllWheelsGrounded())
            {
                Debug.Log("activate drift");
                IsDrift = true;
                Drift(IsDrift);
            }
        }
    }
    #endregion






    #region C# event
    public event Action OnSpeedMetThreshold;
    public event Action OnSpeedGotLowThreshold;
    public event Action<bool> OnDriftSkidMark;
    #endregion





    #region Unity event
    [SerializeField] private UnityEvent OnDriftCharging;
    [SerializeField] private UnityEvent OnReadyToDriftBoost;
    [SerializeField] private UnityEvent OnDriftBoost;
    [SerializeField] private UnityEvent OnDriftEffectOff;
    #endregion






    void Start()
    {
        carRB = GetComponent<Rigidbody>();
        if(local_InputReceiver == null) local_InputReceiver = GetComponent<Local_InputReceiver>();
        local_InputReceiver.OnHorizontalInputChanged += UpdateHorizontalValue;
        local_InputReceiver.OnVerticalInputChanged += UpdateVerticalValue;
        local_InputReceiver.OnDriftInputChanged += UpdateDriftValue;
        ApplyFrictionSettings(IsDrift);
    }
    private void UpdateHorizontalValue(float newValue)
    {
        _horizontalInput = newValue;
    }
    private void UpdateVerticalValue(float newValue)
    {
        _verticalInput = newValue;
    }
    private void UpdateDriftValue(bool newValue)
    {
        _driftKeyPressed = newValue;
    }
    private void FixedUpdate()
    {
        CurrentSpeed = carRB.velocity.magnitude;
        //Debug.Log("Current Speed: " + CurrentSpeed);

        //apply additional gravity to a car
        carRB.AddForce(Vector3.down * additionalGravity, ForceMode.Acceleration);
        UpdateMotor();
        UpdateSteering();
        ControlZAxisRotation();
        Flipping();
        Spinning();
        UpdateAllWheels();
        IsLandedPerfectly();
        DriftBoostCheck(IsDrift);

        // SpeedPad 드리프트 관리
        /*
        if (isDriftFromSpeedPad)
        {
            driftTimer += Time.fixedDeltaTime;
            if (driftTimer >= driftDurationOnSpeedPad || !AllWheelsGrounded())
            {
                // 드리프트 시간 만료 또는 지면 접촉 끊김
                isDriftFromSpeedPad = false;
                IsDrift = false;
                Drift(false);
                OnDriftEffectOff.Invoke();
                // 시간 초기화는 드리프트 시작할 때 하게 뒀음
                Debug.Log("Drift from SpeedPad ended");
            }
        }
        */
    }

    // 스피드 패드와 충돌 시 드리프트 시작
    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SpeedPad") && AllWheelsGrounded())
        {
            Debug.Log("Hit SpeedPad, activating drift");
            isDriftFromSpeedPad = true;
            IsDrift = true;
            Drift(true);
            driftTimer = 0f; // 타이머 초기화
            OnDriftCharging.Invoke(); // 드리프트 효과 시작
        }
    }
    */



    // Functions to manipulate the wheel colliders.
    private void UpdateMotor()
    {
        //Set the Car Rear-Wheel Drive
        if (OneWheelGrounded() && _currentSpeed < maxSelfSpeed)
        {
            for (int i = 2; i < 4; i++)
            {
                wheelColliders[i].motorTorque = _verticalInput * motorTorque;
            }
        }
        else if (OneWheelGrounded())
        {
            for (int i = 2; i < 4; i++)
            {
                wheelColliders[i].motorTorque = 0f;
            }
        }

        _currentBrakeForce = (_verticalInput == 0f && !_isBoosting) ? brakeTorque : 0f;
        ApplyBraking();
    }

    // Function to apply brakes to wheel colliders.
    private void ApplyBraking()
    {
        for (int i = 0; i < 4; i++)
        {
            wheelColliders[i].brakeTorque = _currentBrakeForce;
        }
    }

    // Function that applies steering the handle.
    private void UpdateSteering()
    {
        if (IsDrift)
        {
            _currentsteerAngle = maxSteerAngleWithDrift * _horizontalInput;
        }
        else
        {
            _currentsteerAngle = maxSteerAngle * _horizontalInput;
        }
        for (int i = 0; i < 2; i++)
        {
            wheelColliders[i].steerAngle = _currentsteerAngle;
        }
    }

    private void Flipping()
    {
        if (!OneWheelGrounded() && carRB.isKinematic != true)
        {
            // 공중에 뜬 시간 카운트
            airTimeForFlip += Time.fixedDeltaTime;
            // 일정 시간(flipDelay) 이상 공중에 떠 있을 때만 플립 가능
            if (airTimeForFlip >= flipAndSpinDelay)
            {
                // 1
                /*
                if (_verticalInput != 0)
                {
                    // remove the previous angularVelocity
                    Vector3 angVel = carRB.angularVelocity;
                    angVel.x = 0f;
                    carRB.angularVelocity = angVel;
                }

                float rotationAmount = -1 * _verticalInput * flipForce * Time.fixedDeltaTime;
                Quaternion deltaRotation = Quaternion.Euler(rotationAmount, 0f, 0f);
                carRB.MoveRotation(carRB.rotation * deltaRotation);
                */

                //UnityEngine.Debug.Log("carRB.angularVelocity: " + carRB.angularVelocity);


                // 2
                /*
                // 현재 로컬 회전 속도 확인
                Vector3 localAngularVel = transform.InverseTransformDirection(rb.angularVelocity);

                // 앞뒤 회전 (로컬 X축)
                if (_verticalInput != 0 && Mathf.Abs(localAngularVel.x) < maxFlipAngularSpeed)
                {
                    Vector3 torque = transform.TransformDirection(Vector3.right) * flipTorque;
                    rb.AddTorque(torque, ForceMode.Acceleration);
                }


                // 좌우 스핀 (로컬 Y축)
                if (Input.GetKey(KeyCode.Y) && Mathf.Abs(localAngularVel.y) < maxFlipAngularSpeed)
                {
                    Vector3 torque = transform.TransformDirection(Vector3.up) * flipTorque;
                    rb.AddTorque(torque, ForceMode.Acceleration);
                }
                */

                /*
                // 롤링 (로컬 Z축)
                if (Input.GetKey(KeyCode.Z) && Mathf.Abs(localAngularVel.z) < maxFlipAngularSpeed)
                {
                    Vector3 torque = transform.TransformDirection(Vector3.forward) * flipTorque;
                    rb.AddTorque(torque, ForceMode.Acceleration);
                }
                */

                // 3

                if (_verticalInput != 0)
                {
                    // remove the previous angularVelocity
                    isFlipping = true;
                }
                if (isFlipping)
                {
                    // 현재 월드 기준 각속도 → 로컬 기준으로 변환
                    Vector3 currentLocalAngularVel = transform.InverseTransformDirection(carRB.angularVelocity);

                    // 목표 각속도 계산 (X축 기준)
                    float targetX = -1 * _verticalInput * flipAngularSpeed;  // 예: flipAngularSpeed = Mathf.PI * 2 (1초에 한 바퀴)

                    // Lerp로 보간하여 부드럽게 목표 속도에 접근
                    float newX = Mathf.Lerp(currentLocalAngularVel.x, targetX, Time.fixedDeltaTime * smoothingFactorX);

                    // 새로운 로컬 각속도
                    Vector3 newLocalAngularVel = new Vector3(newX, currentLocalAngularVel.y, currentLocalAngularVel.z);

                    // 다시 월드 기준으로 변환하여 적용
                    carRB.angularVelocity = transform.TransformDirection(newLocalAngularVel);
                }
            }
        }
        else
        {
            airTimeForFlip = 0.0f;
            isFlipping = false;
        }
    }

    // for spinning
    private void Spinning()
    {
        if (!OneWheelGrounded() && carRB.isKinematic != true)
        {
            // 공중에 뜬 시간 카운트
            airTimeForSpin += Time.fixedDeltaTime;
            if (airTimeForSpin >= flipAndSpinDelay)
            {
                // 현재 월드 기준 각속도 → 로컬 기준으로 변환
                Vector3 currentLocalAngularVel = transform.InverseTransformDirection(carRB.angularVelocity);

                if (_horizontalInput > 0.5f)
                {
                    if (currentLocalAngularVel.y <= spinAngularSpeed)
                    {
                        //carRB.AddTorque((transform.up * _horizontalInput * sameDirectionSpinForce), ForceMode.Impulse);

                        // 목표 각속도 계산 (Y축 기준)
                        float targetY = _horizontalInput * spinAngularSpeed;

                        // Lerp로 보간하여 부드럽게 목표 속도에 접근
                        float newY = Mathf.Lerp(currentLocalAngularVel.y, targetY, Time.fixedDeltaTime * smoothingFactorY);

                        // 새로운 로컬 각속도
                        Vector3 newLocalAngularVel = new Vector3(currentLocalAngularVel.x, newY, currentLocalAngularVel.z);

                        // 다시 월드 기준으로 변환하여 적용
                        carRB.angularVelocity = transform.TransformDirection(newLocalAngularVel);

                        if(currentLocalAngularVel.y > 0.0f)
                        {
                            isSpinning = true;
                        }
                    }
                }
                else if (_horizontalInput < -0.5f)
                {
                    /*
                    if (currentLocalAngularVel.y > 0.01f)
                    {
                        //carRB.AddTorque((transform.up * _horizontalInput * oppositeDirectionSpinForce), ForceMode.Impulse);

                        // 목표 각속도 계산 (Y축 기준)
                        float targetY = _horizontalInput * spinAngularSpeed;

                        // Lerp로 보간하여 부드럽게 목표 속도에 접근
                        float newY = Mathf.Lerp(currentLocalAngularVel.y, targetY, Time.fixedDeltaTime * smoothingFactorY);

                        // 새로운 로컬 각속도
                        Vector3 newLocalAngularVel = new Vector3(currentLocalAngularVel.x, newY, currentLocalAngularVel.z);

                        // 다시 월드 기준으로 변환하여 적용
                        carRB.angularVelocity = transform.TransformDirection(newLocalAngularVel);
                    }
                    */
                    if (currentLocalAngularVel.y >= -spinAngularSpeed)
                    {
                        //carRB.AddTorque((transform.up * _horizontalInput * sameDirectionSpinForce), ForceMode.Impulse);

                        // 목표 각속도 계산 (Y축 기준)
                        float targetY = _horizontalInput * spinAngularSpeed;

                        // Lerp로 보간하여 부드럽게 목표 속도에 접근
                        float newY = Mathf.Lerp(currentLocalAngularVel.y, targetY, Time.fixedDeltaTime * smoothingFactorY);

                        // 새로운 로컬 각속도
                        Vector3 newLocalAngularVel = new Vector3(currentLocalAngularVel.x, newY, currentLocalAngularVel.z);

                        // 다시 월드 기준으로 변환하여 적용
                        carRB.angularVelocity = transform.TransformDirection(newLocalAngularVel);

                        if (currentLocalAngularVel.y < 0.0f)
                        {
                            isSpinning = true;
                        }
                    }
                }
                else if (isSpinning == true)
                {
                    // 목표 각속도 계산 (Y축 기준)
                    float targetY = _horizontalInput * spinAngularSpeed;

                    // Lerp로 보간하여 부드럽게 목표 속도에 접근
                    float newY = Mathf.Lerp(currentLocalAngularVel.y, targetY, Time.fixedDeltaTime * smoothingFactorY);

                    // 새로운 로컬 각속도
                    Vector3 newLocalAngularVel = new Vector3(currentLocalAngularVel.x, newY, currentLocalAngularVel.z);

                    // 다시 월드 기준으로 변환하여 적용
                    carRB.angularVelocity = transform.TransformDirection(newLocalAngularVel);
                }
            }

            //UnityEngine.Debug.Log("yAngularSpeed: " + yAngularSpeed);
        }
        else
        {
            airTimeForSpin = 0.0f;
            isSpinning = false;
        }
    }

    private void ControlZAxisRotation()
    {
        if (!OneWheelGrounded() && carRB.isKinematic == false)
        {
            float zAngularSpeed = Vector3.Dot(carRB.angularVelocity, carRB.transform.forward);

            // 절댓값이 임계값 이하인지 확인
            if (Mathf.Abs(zAngularSpeed) < zAngularSpeedThreshold)
            {
                // remove the previous angularVelocity
                Vector3 angVel = carRB.angularVelocity;
                Vector3 localAngVel = carRB.transform.InverseTransformDirection(angVel);
                localAngVel.z = 0f;
                carRB.angularVelocity = carRB.transform.TransformDirection(localAngVel); ;


                /*
                // z축 회전 0으로 만들기
                Quaternion targetRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, 0f);
                carRB.MoveRotation(Quaternion.Lerp(currentRotation, targetRotation, Time.fixedDeltaTime * rotationLerpSpeedForZ));

                float zAngle = carRB.rotation.eulerAngles.z;
                if (zAngle > 180f) zAngle -= 360f; // -180~180도로 정규화
                if (Mathf.Abs(zAngle) < 0.01f)
                {
                    carRB.MoveRotation(Quaternion.Euler(currentEuler.x, currentEuler.y, 0f));
                }
                */

                
                //if (isZFinish == false)
                //{
                    /*
                    // 현재 Z축 회전 (오일러 각도)
                    Quaternion currentRotation = carRB.rotation;
                    Vector3 currentEuler = currentRotation.eulerAngles;

                    // Z축만 0으로 보간, X와 Y는 유지
                    float targetZ = 0f;
                    float newZ = Mathf.Lerp(currentEuler.z, targetZ, Time.fixedDeltaTime * rotationLerpSpeedForZ);
                    transform.eulerAngles = new Vector3(currentEuler.x, currentEuler.y, newZ);


                    // Z축 회전이 거의 0에 가까우면 정지 상태 유지
                    if (Mathf.Abs(newZ) < 0.1f)
                    {
                        transform.eulerAngles = new Vector3(currentEuler.x, currentEuler.y, 0f);
                        isZFinish = true;
                    }
                    */

                    /*
                    float zAngle = carRB.rotation.eulerAngles.z;
                    if (zAngle > 180f) zAngle -= 360f; // -180~180도로 정규화
                    if (Mathf.Abs(zAngle) < 0.1f)
                    {
                        carRB.MoveRotation(Quaternion.Euler(currentEuler.x, currentEuler.y, 0f));
                        isZFinish = true;
                    }
                    */

                    /*
                    if (!isZFinish)
                    {
                        Quaternion currentRotation = carRB.rotation;
                        Vector3 currentEuler = currentRotation.eulerAngles;

                        float currentZ = NormalizeAngle(currentEuler.z); // -180~180 범위로 변환

                        //float targetZ = Mathf.Abs(currentZ) < 90f ? 0f : 180f;
                        float targetZ;
                        if (currentZ < -90f)
                            targetZ = -180f;
                        else if (currentZ > 90f)
                            targetZ = 180f;
                        else
                            targetZ = 0f;

                        // Lerp로 부드럽게 회전 보정
                        float newZ = Mathf.LerpAngle(currentZ, targetZ, Time.fixedDeltaTime * rotationLerpSpeedForZ);

                        // 최종적으로 다시 0~360 범위로 변환해서 적용
                        float appliedZ = PositiveAngle(newZ);

                        //Vector3 targetEuler = new Vector3(currentEuler.x, currentEuler.y, appliedZ);

                        // 적용 방식 선택: transform.eulerAngles 또는 MoveRotation
                        // 방법 1: 직접 설정 (주의: 부자연스러울 수 있음)
                        //transform.eulerAngles = targetEuler;

                        // 방법 2: Rigidbody 회전 적용 (더 자연스러움)
                        //Quaternion targetRotation = Quaternion.Euler(targetEuler);
                        //carRB.MoveRotation(targetRotation);

                        // 방법 3: 걍 한 번에 하자고
                        Quaternion targetRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, appliedZ);
                        carRB.MoveRotation(targetRotation);

                        if (Mathf.Abs(currentZ - targetZ) < 1.0f)
                        {
                            //transform.eulerAngles = new Vector3(currentEuler.x, currentEuler.y, 0f);
                            isZFinish = true;
                        }
                    }
                    
                    
                }
                */
                
            }
        }
        else
        {
            isZFinish = false;
        }
    }

    // -180 ~ 180으로 정규화
    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    // 0 ~ 360으로 정규화
    private float PositiveAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0f) angle += 360f;
        return angle;
    }

    // Function that updates the rotation state of the wheels.
    private void UpdateAllWheels()
    {
        for (int i = 0; i < 4; i++)
        {
            UpdateSingleWheel(wheelColliders[i], wheelTransforms[i]);
        }
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    public bool IsGrounded()
    {
        return OneWheelGrounded();
    }

    // Function that determines whether the car is attached to the ground.
    private bool AllWheelsGrounded()
    {
        for (int i = 0; i < 4; i++)
        {
            if (IsWheelGrounded(wheelColliders[i]) == false)
            {
                OnDriftEffectOff.Invoke();
                return false;
            }
        }
        return true;
    }

    private bool OneWheelGrounded()
    {
        for (int i = 0; i < 4; i++)
        {
            if (IsWheelGrounded(wheelColliders[i]) == true)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsWheelGrounded(WheelCollider wheelCollider)
    {
        WheelHit hit;
        return wheelCollider.GetGroundHit(out hit);
    }

    private void IsLandedPerfectly()
    {
        if (!OneWheelGrounded())
        {
            _wasGrounded = false;
            _landingTime = 0.0f;
        }
        else if (!_wasGrounded)
        {
            if (!AllWheelsGrounded())
            {
                _landingTime += Time.fixedDeltaTime;
            }
            else
            {
                UnityEngine.Debug.Log("Landing Time: " + _landingTime);
                if (_landingTime < landingThresholdTime)
                {
                    PerfectLanding();
                }
                _wasGrounded = true;
            }
        }
    }

    private void PerfectLanding()
    {
        UnityEngine.Debug.Log("Perfect Landing!!");
        Boost(LANDING_BOOST);
    }
    private void Boost(int typeOfBoost)
    {
        _isBoosting = true;
        if (typeOfBoost == LANDING_BOOST)
        {
            // Invoke DriftBoost Effect **ONCE**
            OnDriftBoost.Invoke();

            // 기존 속도에서 전방 성분만 남김
            carRB.velocity = Vector3.Project(carRB.velocity, transform.forward);

            carRB.AddForce(transform.forward * landingBoostSpeed, ForceMode.VelocityChange);
            StartCoroutine(BoostTimer(LANDING_BOOST));
        }
        else if (typeOfBoost == DRIFT_BOOST)
        {
            // Invoke DriftBoost Effect **ONCE**
            OnDriftBoost.Invoke();

            // 기존 속도에서 전방 성분만 남김
            carRB.velocity = Vector3.Project(carRB.velocity, transform.forward);

            carRB.AddForce(transform.forward * driftBoostSpeed, ForceMode.VelocityChange);
            StartCoroutine(BoostTimer(DRIFT_BOOST));
        }
    }
    private IEnumerator BoostTimer(int typeOfBoost)
    {
        if (typeOfBoost == LANDING_BOOST)
        {
            yield return new WaitForSeconds(boostTimeOfLanding);
        }
        else if (typeOfBoost == DRIFT_BOOST)
        {
            yield return new WaitForSeconds(boostTimeOfDrift);
        }

        _isBoosting = false;
    }
    private void BrakeWhileDrift(bool activate)
    {
        if (activate)
        {
            wheelColliders[2].brakeTorque = wheelColliders[3].brakeTorque = 100000;
            wheelColliders[3].brakeTorque = wheelColliders[3].brakeTorque = 100000;
        }
        else
        {
            wheelColliders[2].brakeTorque = wheelColliders[3].brakeTorque = brakeTorque;
            wheelColliders[3].brakeTorque = wheelColliders[3].brakeTorque = brakeTorque;
        }
    }

    public void ApplyFrictionSettings(bool isDrift)
    {
        for (int i = 2; i < 4; i++)
        {
            // Forward Friction 설정
            WheelFrictionCurve forwardFriction = wheelColliders[i].forwardFriction;
            forwardFriction.extremumSlip = isDrift ? driftForwardExtremumSlip : defaultForwardExtremumSlip;
            forwardFriction.extremumValue = isDrift ? driftForwardExtremumValue : defaultForwardExtremumValue;
            forwardFriction.asymptoteSlip = isDrift ? driftForwardAsymptoteSlip : defaultForwardAsymptoteSlip;
            forwardFriction.asymptoteValue = isDrift ? driftForwardAsymptoteValue : defaultForwardAsymptoteValue;
            forwardFriction.stiffness = isDrift ? driftForwardStiffness : defaultForwardStiffness;
            wheelColliders[i].forwardFriction = forwardFriction;

            // Sideways Friction 설정
            WheelFrictionCurve sidewaysFriction = wheelColliders[i].sidewaysFriction;
            sidewaysFriction.extremumSlip = isDrift ? driftSideExtremumSlip : defaultSideExtremumSlip;
            sidewaysFriction.extremumValue = isDrift ? driftSideExtremumValue : defaultSideExtremumValue;
            sidewaysFriction.asymptoteSlip = isDrift ? driftSideAsymptoteSlip : defaultSideAsymptoteSlip;
            sidewaysFriction.asymptoteValue = isDrift ? driftSideAsymptoteValue : defaultSideAsymptoteValue;
            sidewaysFriction.stiffness = isDrift ? driftSideStiffness : defaultSideStiffness;
            wheelColliders[i].sidewaysFriction = sidewaysFriction;
        }
    }
    public void ApplyBrakeForFinishInMulti()
    {
        StartCoroutine(SmoothIncreaseBrakeTorque(100000f, 1.5f));
    }

    private IEnumerator SmoothIncreaseBrakeTorque(float targetTorque, float duration)
    {
        float startTorque = brakeTorque;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            brakeTorque = Mathf.Lerp(startTorque, targetTorque, t);
            yield return null;
        }

        brakeTorque = targetTorque; // 정확히 도달 보장
    }
    private void Drift(bool activate)
    {
        if (activate)
        {
            BrakeWhileDrift(true);
            ApplyFrictionSettings(true);
        }
        else
        {
            BrakeWhileDrift(false);
            ApplyFrictionSettings(false);
            //if (_frictionRecoveryCoroutine != null) StopCoroutine(_frictionRecoveryCoroutine);
            //_frictionRecoveryCoroutine = StartCoroutine(RestoreFrictionOverTime(driftRecoverTime));
        }
    }

    // measure drifting time
    private void DriftBoostCheck(bool isDrift)
    {
        if (isDrift)
        {
            if (AllWheelsGrounded())
            {
                
                _driftTime += Time.fixedDeltaTime;

                if (_driftTime >= timeThresholdToDriftBoost)
                {
                    if (OkToDriftBoost == false)
                    {
                        // Invoke ReadyToBoost Effect **ONCE**
                        OnReadyToDriftBoost.Invoke();
                    }
                    OkToDriftBoost = true;
                }
                //if user unpresses the gas while drift, drift is got off
                if (_verticalInput == 0)
                {
                    _driftPause = true;
                    //Debug.Log("vertical changed to 0, check whether to boost");
                }
                DriftBoost();
            }
            else
            {
                OnDriftEffectOff.Invoke();
                IsDrift = false;
                Drift(IsDrift);
                //isDriftFromSpeedPad = false; // SpeedPad 드리프트도 종료
            }
        }
    }
    private void DriftBoost()
    {
        //if user pressed the gas again
        if (_driftPause == true && _verticalInput == 1 && OkToDriftBoost)
        {
            _driftPause = false;
            IsDrift = false;
            _driftTime = 0;
            Drift(false);
            Debug.Log("Drift Boost! & Drift Deactivated & Initalize DriftTime");
            OkToDriftBoost = false;
            Boost(DRIFT_BOOST);
        }
    }
    public void EnbleCarControll(bool active)
    {
        playerInput.enabled = active;
    }
    public void ResetCarPos(bool isMulti, Transform reStart)
    {
        if (isMulti)
        {
            carRB.isKinematic = true;
            this.transform.SetPositionAndRotation(reStart.position, reStart.rotation);
            //delay 필요
            carRB.isKinematic = false;
        }
        else
        {
            carRB.isKinematic = true;

            this.transform.SetPositionAndRotation(StartPoint.position, StartPoint.rotation);
            StartCylinder.SetActive(true);
            this.transform.SetParent(StartPoint);
        }
    }

    private IEnumerator RestoreFrictionOverTime(float duration)
    {
        float t = 0f;

        // 현재 마찰값 저장
        WheelFrictionCurve startForwardFriction = wheelColliders[2].forwardFriction;
        WheelFrictionCurve startSidewaysFriction = wheelColliders[2].sidewaysFriction;

        while (t < duration)
        {
            float lerpT = t / duration;

            for (int i = 2; i < 4; i++)
            {
                WheelFrictionCurve forward = wheelColliders[i].forwardFriction;
                WheelFrictionCurve sideways = wheelColliders[i].sidewaysFriction;

                // Forward 보간
                forward.stiffness = Mathf.Lerp(startForwardFriction.stiffness, defaultForwardStiffness, lerpT);
                forward.extremumSlip = Mathf.Lerp(startForwardFriction.extremumSlip, defaultForwardExtremumSlip, lerpT);
                forward.extremumValue = Mathf.Lerp(startForwardFriction.extremumValue, defaultForwardExtremumValue, lerpT);
                forward.asymptoteSlip = Mathf.Lerp(startForwardFriction.asymptoteSlip, defaultForwardAsymptoteSlip, lerpT);
                forward.asymptoteValue = Mathf.Lerp(startForwardFriction.asymptoteValue, defaultForwardAsymptoteValue, lerpT);

                // Side 보간
                sideways.stiffness = Mathf.Lerp(startSidewaysFriction.stiffness, defaultSideStiffness, lerpT);
                sideways.extremumSlip = Mathf.Lerp(startSidewaysFriction.extremumSlip, defaultSideExtremumSlip, lerpT);
                sideways.extremumValue = Mathf.Lerp(startSidewaysFriction.extremumValue, defaultSideExtremumValue, lerpT);
                sideways.asymptoteSlip = Mathf.Lerp(startSidewaysFriction.asymptoteSlip, defaultSideAsymptoteSlip, lerpT);
                sideways.asymptoteValue = Mathf.Lerp(startSidewaysFriction.asymptoteValue, defaultSideAsymptoteValue, lerpT);

                wheelColliders[i].forwardFriction = forward;
                wheelColliders[i].sidewaysFriction = sideways;
            }

            t += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // 최종값 보정
        ApplyFrictionSettings(false);
    }
}
