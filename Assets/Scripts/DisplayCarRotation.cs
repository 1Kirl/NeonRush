using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayCarRotation : MonoBehaviour
{
    [Header("회전 속도 (도/초)")]
    [SerializeField] private float rotationSpeed = 90f;

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
