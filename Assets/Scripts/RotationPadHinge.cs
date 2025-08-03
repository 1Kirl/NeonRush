using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(HingeJoint))]
public class RotationPadHinge : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float delayTime = 1.0f;      // Ʈ���� �� ȸ�� ���۱����� ������
    [SerializeField] private float rotateDuration = 1.5f; // ȸ�� ���� �ð�
    [SerializeField] private float resetDelay = 2.0f;     // ����ġ ��� �ð�

    [Header("Rotation")]
    [SerializeField] private float rotateSpeed = 300.0f;  // ���� �ӵ�
    [SerializeField] private float rotateForce = 1000.0f; // ���� ��

    private HingeJoint _hinge;
    private JointMotor _motor;
    private bool _isReady = true;

    private void Start()
    {
        _hinge = GetComponent<HingeJoint>();
        _hinge.useMotor = false; // �ʱ⿡�� ��Ȱ��ȭ
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _isReady)
        {
            Debug.Log("Hinge Rotation Pad triggered");
            _isReady = false;
            StartCoroutine(RotateSequence());
        }
    }

    private IEnumerator RotateSequence()
    {
        yield return new WaitForSeconds(delayTime);

        Debug.Log("Rotation starts");

        // ���� ����
        _motor = _hinge.motor;
        _motor.force = rotateForce;
        _motor.targetVelocity = rotateSpeed;
        _hinge.motor = _motor;
        _hinge.useMotor = true;

        yield return new WaitForSeconds(rotateDuration);

        Debug.Log("Rotation ends");

        // ȸ�� ����
        _motor.targetVelocity = 0;
        _hinge.motor = _motor;
        _hinge.useMotor = false;

        yield return new WaitForSeconds(resetDelay);

        Debug.Log("Returning to start");

        // �ݴ� �������� ȸ���ؼ� ����ġ
        _motor.targetVelocity = -rotateSpeed;
        _hinge.motor = _motor;
        _hinge.useMotor = true;

        yield return new WaitForSeconds(rotateDuration);

        // ������ ����
        _motor.targetVelocity = 0;
        _hinge.motor = _motor;
        _hinge.useMotor = false;

        _isReady = true;
        Debug.Log("Pad ready again");
    }
}
