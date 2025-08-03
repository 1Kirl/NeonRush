using UnityEngine;

public class SawBlade : MonoBehaviour
{
    public float rotationSpeed = 360f; // 1�ʿ� 360�� ȸ��

    void Update() {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 2f); // ȸ���� �ð�ȭ
    }


}
