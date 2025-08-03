using UnityEngine;

public class GearMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveDirection = Vector3.right;
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveSpeed = 3f;

    private Vector3 startPos;
    private float time;

    void Start() {
        startPos = transform.position;
        moveDirection.Normalize();
    }

    void Update() {
        time += Time.deltaTime;
        float lerpFactor = Mathf.PingPong(time * moveSpeed, 1f);
        transform.position = startPos + moveDirection * (lerpFactor * moveDistance);
    }
}
