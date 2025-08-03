using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformUpdateController : MonoBehaviour
{
    [SerializeField] private int _clientId = 99;
    [SerializeField] private float baseSmoothing = 10f;
    [SerializeField] private float teleportThreshold = 5.0f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 lastReceivedPosition;
    private float lastReceivedTime;

    private float estimatedSpeed = 0f; // m/s

    public int ClientId
    {
        get => _clientId;
        set
        {
            Debug.Log($"[IR] InputReceiver and Client Id Binded. Id: {value}");
            _clientId = value;
            LiteNetLibManager.Instance.clientsDic[value].OnTransformUpdate += UpdateTransform;
        }
    }

    public void UpdateTransform(Vector3 pos, Quaternion rot)
    {
        float now = Time.time;
        float dt = now - lastReceivedTime;

        if (dt > 0f)
        {
            estimatedSpeed = Vector3.Distance(pos, lastReceivedPosition) / dt;
        }

        lastReceivedPosition = pos;
        lastReceivedTime = now;

        targetPosition = pos;
        targetRotation = rot;
    }

    private void Update()
    {
        float speedMultiplier = Mathf.Clamp01(estimatedSpeed / 10f); // max 10m/s 기준
        float dynamicSmoothing = Mathf.Lerp(baseSmoothing * 0.5f, baseSmoothing * 2f, speedMultiplier);

        if (Vector3.Distance(transform.position, targetPosition) > teleportThreshold)
        {
            transform.position = targetPosition;
        }
        else
        {
            transform.position = Vector3.Lerp(
                                        transform.position,
                                        targetPosition,
                                        Time.deltaTime * dynamicSmoothing);
            transform.rotation = Quaternion.Slerp(
                                            transform.rotation,
                                            targetRotation,
                                            Time.deltaTime * dynamicSmoothing);
        }
    }
} 