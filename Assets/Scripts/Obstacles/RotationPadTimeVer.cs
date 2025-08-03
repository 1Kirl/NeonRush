using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationPadTimeVer : MonoBehaviour
{
    #region serialize variable

    //[SerializeField] private float endXAngle = 45.0f;

    //[SerializeField] private float endYAngle = 0.0f;

    //[SerializeField] private float endZAngle = 0.0f;

    [SerializeField] private float delayTime = 1.0f;

    //[SerializeField] private float rotationSpeed = 180.0f;

    [SerializeField] private float time = 2.0f;

    [SerializeField] private Transform endTransform;

    [SerializeField] private float resetDelay = 2.0f;

    #endregion





    #region private variable

    private Quaternion _startAngle;

    private Quaternion _endAngle;

    private Rigidbody _rigidBody;

    private bool _isRotationStarted = false;

    private bool _isReturning = false;

    private float _currentTime = 0.0f;

    private bool _isReady = true;

    #endregion





    #region private funcs

    private void Start()
    {
        _rigidBody = this.GetComponent<Rigidbody>();
        //_endAngle = Quaternion.Euler(endXAngle, endYAngle, endZAngle);
        _startAngle = this.transform.rotation;
        _endAngle = endTransform.rotation;
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
        if (_isRotationStarted)
        {
            /*
            Quaternion newRot = Quaternion.RotateTowards(_rigidbBody.rotation, _endAngle, rotationSpeed * Time.fixedDeltaTime);
            _rigidbBody.MoveRotation(newRot);
            */
            /*
            _currentTime += Time.fixedDeltaTime;
            float t = _currentTime / time;

            Quaternion newRotation = Quaternion.Slerp(_startAngle, _endAngle, t);
            _rigidBody.MoveRotation(newRotation);
            if (t >= 1.0f)
            {
                _rigidBody.MoveRotation(_endAngle);
            }
            */
            _currentTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(_currentTime / time);
            Quaternion targetRotation = Quaternion.Slerp(_startAngle, _endAngle, t);
            _rigidBody.MoveRotation(targetRotation);

            if (t >= 1.0f)
            {
                _rigidBody.MoveRotation(_endAngle);
                _isRotationStarted = false;
                StartCoroutine(ResetAfterDelay());
            }
        }
        else if (_isReturning)
        {
            _currentTime += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(_currentTime / time);
            Quaternion targetRotation = Quaternion.Slerp(_endAngle, _startAngle, t);
            _rigidBody.MoveRotation(targetRotation);

            if (t >= 1.0f)
            {
                _rigidBody.MoveRotation(_startAngle);
                _isReturning = false;
                _isReady = true;
            }
        }
    }

    private IEnumerator StartRotation()
    {
        yield return new WaitForSeconds(delayTime);
        UnityEngine.Debug.Log("Rotation starts");
        _isRotationStarted = true;
        _currentTime = 0.0f;
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        _isReturning = true;
        _currentTime = 0.0f;
    }

    #endregion
}
