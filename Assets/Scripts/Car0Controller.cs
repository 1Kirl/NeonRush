using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car0Controller : MonoBehaviour
{
    public WheelCollider frontLeftWheel, frontRightWheel;
    public WheelCollider backLeftWheel, backRightWheel;
    public Transform frontLeftTransform, frontRightTransform;
    public Transform backLeftTransform, backRightTransform;

    public float maxTorque = 1500f; // �ִ� ���� ��
    public float maxSteerAngle = 30f; // �ִ� ���� ����

    private void FixedUpdate()
    {
        float vertical = Input.GetAxis("Vertical"); // ����/����
        float horizontal = Input.GetAxis("Horizontal"); // �¿� ����

        // ���� ������ �� ���� (�޹��� ����)
        backLeftWheel.motorTorque = vertical * maxTorque;
        backRightWheel.motorTorque = vertical * maxTorque;

        // �չ��� ����
        frontLeftWheel.steerAngle = horizontal * maxSteerAngle;
        frontRightWheel.steerAngle = horizontal * maxSteerAngle;

        // ���� ��ġ ������Ʈ (�׷��� �ݿ�)
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
