using UnityEngine;

public class RotationBlock : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float maxAngle = -120f;
    [SerializeField] private float swingSpeed = 2f;
    [SerializeField] private float rotationOffset = -180f;
    [SerializeField] private float phaseOffset = 0f;

    [Header("Rotation Axis")]
    [SerializeField] private bool rotateX = false;
    [SerializeField] private bool rotateY = false;
    [SerializeField] private bool rotateZ = true;

    private float time;
    private Quaternion initialRotation;

    void Start() {
        initialRotation = transform.rotation; // 초기 회전값 저장
    }

    void FixedUpdate() {
        time += Time.fixedDeltaTime;
        float angle = maxAngle * Mathf.Sin(time * swingSpeed + phaseOffset) + rotationOffset;

        Vector3 eulerOffset = Vector3.zero;
        if (rotateX) eulerOffset.x = angle;
        if (rotateY) eulerOffset.y = angle;
        if (rotateZ) eulerOffset.z = angle;

        // 회전: 초기 회전 * 추가 회전
        transform.rotation = initialRotation * Quaternion.Euler(eulerOffset);
    }
}
