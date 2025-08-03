using UnityEngine;

public class AxisOscillator : MonoBehaviour
{
    [Header("Oscillation Settings")]
    [SerializeField] private Vector3 axis = Vector3.right; // 진동 축
    [SerializeField] private float amplitude = 1f;          // 최대 이동 거리
    [SerializeField] private float speed = 1f;              // 속도
    [SerializeField] private bool invert = false;           // 반대 방향으로 움직이게 할지 여부

    private Vector3 centerPosition;
    private float time;

    void Start() {
        centerPosition = transform.position;
        axis.Normalize();
    }

    void Update() {
        time += Time.deltaTime;

        // -1~1 사이의 사인 곡선 → 반전 가능
        float directionFactor = invert ? -1f : 1f;
        float offset = Mathf.Sin(time * speed) * amplitude * directionFactor;

        transform.position = centerPosition + axis * offset;
    }
}
