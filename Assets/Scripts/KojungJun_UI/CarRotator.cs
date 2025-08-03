using UnityEngine;

public class CarRotator : MonoBehaviour
{
    public float rotationSpeed = 20f;

    public void RotateOnce() {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    public void StopAndReset() {
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public void ResetRotation() {
        transform.rotation = Quaternion.identity;
    }

}
