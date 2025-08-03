using UnityEngine;

public class SawBlade : MonoBehaviour
{
    public float rotationSpeed = 360f; // 1초에 360도 회전

    void Update() {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 2f); // 회전축 시각화
    }


}
