using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriftController : MonoBehaviour
{
    [Header("WheelColliders: [0],[1]: Front / [2],[3]: Rear")]
    // Wheel colliders. physically interact with the environment.
    [SerializeField] private WheelCollider[] wheelColliders = new WheelCollider[4];

    [Header("Frictions")]
    [SerializeField] private float forwardFriction;
    [SerializeField] private float sidewayFriction;

    public void setupWheelCollider(GameObject element, int index)
    {
        WheelFrictionCurve curve = wheelColliders[2].forwardFriction;

        curve = wheelColliders[3].forwardFriction;
    
    }
}
