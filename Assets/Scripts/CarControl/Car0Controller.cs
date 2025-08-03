using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car0Controller : MonoBehaviour
{
    public WheelCollider frontLeftWheel, frontRightWheel;
    public WheelCollider backLeftWheel, backRightWheel;
    public Transform frontLeftTransform, frontRightTransform;
    public Transform backLeftTransform, backRightTransform;

    public float maxTorque = 1500f; // 최대 엔진 힘
    public float maxSteerAngle = 30f; // 최대 조향 각도

    private void FixedUpdate()
    {
        float vertical = Input.GetAxis("Vertical"); // 전진/후진
        float horizontal = Input.GetAxis("Horizontal"); // 좌우 조향

        // 구동 바퀴에 힘 전달 (뒷바퀴 구동)
        backLeftWheel.motorTorque = vertical * maxTorque;
        backRightWheel.motorTorque = vertical * maxTorque;

        // 앞바퀴 조향
        frontLeftWheel.steerAngle = horizontal * maxSteerAngle;
        frontRightWheel.steerAngle = horizontal * maxSteerAngle;

        // 바퀴 위치 업데이트 (그래픽 반영)
        UpdateWheelPose(frontLeftWheel, frontLeftTransform);
        UpdateWheelPose(frontRightWheel, frontRightTransform);
        UpdateWheelPose(backLeftWheel, backLeftTransform);
        UpdateWheelPose(backRightWheel, backRightTransform);
    }

    private void UpdateWheelPose(WheelCollider collider, Transform trans)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        trans.position = pos;
        trans.rotation = rot;
    }
}
