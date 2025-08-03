using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(HingeJoint))]
public class RotationPadHinge : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float delayTime = 1.0f;      // 트리거 후 회전 시작까지의 딜레이
    [SerializeField] private float rotateDuration = 1.5f; // 회전 유지 시간
    [SerializeField] private float resetDelay = 2.0f;     // 원위치 대기 시간

    [Header("Rotation")]
    [SerializeField] private float rotateSpeed = 300.0f;  // 모터 속도
    [SerializeField] private float rotateForce = 1000.0f; // 모터 힘

    private HingeJoint _hinge;
    private JointMotor _motor;
    private bool _isReady = true;

    private void Start()
    {
        _hinge = GetComponent<HingeJoint>();
        _hinge.useMotor = false; // 초기에는 비활성화
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

        // 모터 설정
        _motor = _hinge.motor;
        _motor.force = rotateForce;
        _motor.targetVelocity = rotateSpeed;
        _hinge.motor = _motor;
        _hinge.useMotor = true;

        yield return new WaitForSeconds(rotateDuration);

        Debug.Log("Rotation ends");

        // 회전 정지
        _motor.targetVelocity = 0;
        _hinge.motor = _motor;
        _hinge.useMotor = false;

        yield return new WaitForSeconds(resetDelay);

        Debug.Log("Returning to start");

        // 반대 방향으로 회전해서 원위치
        _motor.targetVelocity = -rotateSpeed;
        _hinge.motor = _motor;
        _hinge.useMotor = true;

        yield return new WaitForSeconds(rotateDuration);

        // 완전히 정지
        _motor.targetVelocity = 0;
        _hinge.motor = _motor;
        _hinge.useMotor = false;

        _isReady = true;
        Debug.Log("Pad ready again");
    }
}
