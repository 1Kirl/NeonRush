using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationPad : MonoBehaviour
{
    #region serialize variable

    [SerializeField] private float endXAngle = 45.0f;

    [SerializeField] private float endYAngle = 0.0f;

    [SerializeField] private float endZAngle = 0.0f;

    [SerializeField] private float delayTime = 1.0f;

    [SerializeField] private float rotationSpeed = 180.0f;

    [SerializeField] private float resetDelay = 2.0f;

    #endregion





    #region private variable

    private Quaternion _startAngle;

    private Quaternion _endAngle;

    private Rigidbody _rigidbBody;

    private bool _isRotatingForward = false;

    private bool _isReturning = false;

    private bool _isReady = true;

    #endregion





    #region private funcs

    private void Start()
    {
        _rigidbBody = this.GetComponent<Rigidbody>();
        _startAngle = transform.rotation;
        _endAngle = Quaternion.Euler(endXAngle, endYAngle, endZAngle);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && _isReady)
        {
            UnityEngine.Debug.Log("RotationPad is triggered");
            _isReady = false;
            StartCoroutine(StartRotation());
        }
    }

    private void FixedUpdate()
    {
        /*
        if (_isRotationStarted && _rigidbBody.rotation != _endAngle)
        {
            Quaternion newRot = Quaternion.RotateTowards(_rigidbBody.rotation, _endAngle, rotationSpeed * Time.fixedDeltaTime);
            _rigidbBody.MoveRotation(newRot);
        }
        */
        if (_isRotatingForward)
        {
            Quaternion newRot = Quaternion.RotateTowards(_rigidbBody.rotation, _endAngle, rotationSpeed * Time.fixedDeltaTime);
            _rigidbBody.MoveRotation(newRot);

            if (Quaternion.Angle(_rigidbBody.rotation, _endAngle) < 0.1f)
            {
                _rigidbBody.MoveRotation(_endAngle);
                _isRotatingForward = false;
                StartCoroutine(ResetAfterDelay());
            }
        }
        else if (_isReturning)
        {
            Quaternion newRot = Quaternion.RotateTowards(_rigidbBody.rotation, _startAngle, rotationSpeed * Time.fixedDeltaTime);
            _rigidbBody.MoveRotation(newRot);

            if (Quaternion.Angle(_rigidbBody.rotation, _startAngle) < 0.1f)
            {
                _rigidbBody.MoveRotation(_startAngle);
                _isReturning = false;
                _isReady = true;
            }
        }
    }

    private IEnumerator StartRotation()
    {
        yield return new WaitForSeconds(delayTime);
        UnityEngine.Debug.Log("Rotation starts");
        _isRotatingForward = true;
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        _isReturning = true;
    }

    #endregion
}
