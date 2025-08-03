using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilpTargetController : MonoBehaviour
{
    private GameObject _car;
    private Quaternion targetRotation;
    [SerializeField] private float thresholdSpeed = 20f;

    public GameObject Car
    {
        get => _car;
        set
        {
            _car = value;
            carRigidbody = value.GetComponent<Rigidbody>();
        }
    }
    [SerializeField] private Rigidbody carRigidbody;

    // Update is called once per frame
    void Update()
    {
        if (carRigidbody == null) return;

        Vector3 velocity = carRigidbody.velocity;

        // 수직 방향 제거 (수평 방향만)
        velocity.y = 0f;

        // 웬만하면 그자리에서 가져옴
        if (velocity.sqrMagnitude < thresholdSpeed)
        {
            float carY = Car.transform.eulerAngles.y;
            Vector3 myEuler = transform.eulerAngles;
            targetRotation = Quaternion.Euler(myEuler.x, carY, myEuler.z);
        }
        else
        {
            // velocity 방향을 바라보도록 회전
            targetRotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
        }
            this.transform.position = Car.transform.position;
            transform.rotation = targetRotation;

    }
}
