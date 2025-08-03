using UnityEngine;

public class AxisOscillator : MonoBehaviour
{
    [Header("Oscillation Settings")]
    [SerializeField] private Vector3 axis = Vector3.right; // ���� ��
    [SerializeField] private float amplitude = 1f;          // �ִ� �̵� �Ÿ�
    [SerializeField] private float speed = 1f;              // �ӵ�
    [SerializeField] private bool invert = false;           // �ݴ� �������� �����̰� ���� ����

    private Vector3 centerPosition;
    private float time;

    void Start() {
        centerPosition = transform.position;
        axis.Normalize();
    }

    void Update() {
        time += Time.deltaTime;

        // -1~1 ������ ���� � �� ���� ����
        float directionFactor = invert ? -1f : 1f;
        float offset = Mathf.Sin(time * speed) * amplitude * directionFactor;

        transform.position = centerPosition + axis * offset;
    }
}
