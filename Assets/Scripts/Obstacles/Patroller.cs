using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patroller : MonoBehaviour
{
    [SerializeField] private float amplitude = 2.0f;
    [SerializeField] private float frequency = 1.0f;
    public Vector3 direction = Vector3.forward;

    private Vector3 startPosition;
    private Rigidbody rb;
    
    void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody>();
    }
    
    void FixedUpdate()  // Update 대신 FixedUpdate 사용
    {
        float offset = Mathf.Sin(Time.fixedTime * frequency) * amplitude;
        Vector3 targetPosition = startPosition + direction.normalized * offset;
        
        rb.MovePosition(targetPosition);
    }
}
