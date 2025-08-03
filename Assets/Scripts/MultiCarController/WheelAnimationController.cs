using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelAnimationController : MonoBehaviour
{
    [SerializeField] private InputReceiver inputReceiver;

    [Header("바퀴 계층 참조")]
    [SerializeField] private Transform[] frontWheelBases;  // Y축 조향용 (부모)
    [SerializeField] private Transform[] frontWheelMeshes; // X축 회전용 (자식)
    [SerializeField] private Transform[] backWheels;        // X축 회전용 (뒤 바퀴)

    [Header("샘플 바퀴 및 설정")]
    [SerializeField] private Transform sampleWheel;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private float wheelRadius = 0.35f;
    [SerializeField] private float rotationSpeedMultiplier = 1.0f;
    [SerializeField] private float accelerationRate = 5f;
    [SerializeField] private float decelerationRate = 2f;

    private float _horizontalInput;
    private float _verticalInput;
    private float currentWheelSpeed = 0f;

    void Start()
    {
        if (sampleWheel != null)
        {
            wheelRadius = sampleWheel.GetComponent<MeshRenderer>().bounds.size.y * 0.5f;
        }
    }

    public void BindInputReceiver()
    {
        inputReceiver = GetComponent<InputReceiver>();
        if (inputReceiver != null)
        {
            inputReceiver.OnHorizontalInputChanged += UpdateHorizontalValue;
            inputReceiver.OnVerticalInputChanged += UpdateVerticalValue;
        }
    }

    private void UpdateHorizontalValue(float newValue)
    {
        _horizontalInput = newValue;
    }

    private void UpdateVerticalValue(float newValue)
    {
        _verticalInput = newValue;
    }

    void Update()
    {
        ApplySteering();
        RotateWheels();
    }

    void ApplySteering()
    {
        float steerAngle = maxSteerAngle * _horizontalInput;
        foreach (Transform baseTransform in frontWheelBases)
        {
            baseTransform.localRotation = Quaternion.Euler(0f, steerAngle, 0f);
        }
    }

    void RotateWheels()
    {
        float wheelCircumference = 2 * Mathf.PI * wheelRadius;
        float targetSpeed = _verticalInput * rotationSpeedMultiplier;

        currentWheelSpeed = Mathf.MoveTowards(
            currentWheelSpeed,
            _verticalInput != 0 ? targetSpeed : 0f,
            (_verticalInput != 0 ? accelerationRate : decelerationRate) * Time.deltaTime
        );

        float anglePerFrame = (currentWheelSpeed / wheelCircumference) * 360f * Time.deltaTime;

        // 앞바퀴 회전
        foreach (Transform wheel in frontWheelMeshes)
        {
            wheel.Rotate(Vector3.right, anglePerFrame, Space.Self);
        }

        // 뒷바퀴 회전
        foreach (Transform wheel in backWheels)
        {
            wheel.Rotate(Vector3.right, anglePerFrame, Space.Self);
        }
    }
}