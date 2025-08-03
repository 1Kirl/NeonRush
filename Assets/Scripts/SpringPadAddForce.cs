using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringPadAddForce : MonoBehaviour
{
    [SerializeField] private float springForce = 1000f;
    [SerializeField] private float delayTime = 1.0f;
    [SerializeField] private float resetDelay = 2.0f;

    private Vector3 _startPosition;
    private Rigidbody _rigidBody;
    private bool _isReady = true;

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _startPosition = transform.position;
        _rigidBody.isKinematic = false;
        _rigidBody.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isReady) return;
        if (other.CompareTag("Player"))
        {
            UnityEngine.Debug.Log("SpringPad is triggered");
            StartCoroutine(ActivateSpring());
        }
    }

    private IEnumerator ActivateSpring()
    {
        UnityEngine.Debug.Log("SpringPad triggered. Waiting...");
        _isReady = false;
        yield return new WaitForSeconds(delayTime);
        UnityEngine.Debug.Log("Spring is activated");
        _rigidBody.velocity = Vector3.zero;
        _rigidBody.AddForce(Vector3.up * springForce, ForceMode.VelocityChange);
        yield return new WaitForSeconds(resetDelay);
        UnityEngine.Debug.Log("Spring resetting...");
        ResetPad();
    }

    private void ResetPad()
    {
        _rigidBody.velocity = Vector3.zero;
        _rigidBody.MovePosition(_startPosition);
        _isReady = true;
    }

}
