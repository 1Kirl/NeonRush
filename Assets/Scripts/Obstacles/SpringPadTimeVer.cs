using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringPadTimeVer : MonoBehaviour
{
    #region serialize variable

    //[SerializeField] private float endHeight = 10.0f;

    //[SerializeField] private float movingSpeed = 5.0f;

    [SerializeField] private float delayTime = 1.0f;

    [SerializeField] private float time = 2.0f;

    [SerializeField] private Transform endTransform;

    [SerializeField] private float resetDelay = 2.0f;

    #endregion





    #region private variable

    private Vector3 _startPosition;

    private Vector3 _endPosition;

    private Rigidbody _rigidBody;

    private bool _isSpringActivated = false;

    private float _currentTime = 0.0f;

    private bool _isReturning = false;

    private bool _isReady = true;

    #endregion





    #region private funcs

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _startPosition = this.transform.position;
        _endPosition = endTransform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isReady) return;

        if (other.gameObject.CompareTag("Player"))
        {
            UnityEngine.Debug.Log("SpringPad is triggered");
            _isReady = false;
            StartCoroutine(ActivateSpring());
        }
    }

    private void FixedUpdate()
    {
        if (_isSpringActivated && !_isReturning)
        {
            /*
            Vector3 newPos = Vector3.MoveTowards(_rigidBody.position, _endPosition, movingSpeed * Time.fixedDeltaTime);
            _rigidBody.MovePosition(newPos);
            */

            _currentTime += Time.fixedDeltaTime;
            float t = _currentTime / time;

            Vector3 newPos = Vector3.Lerp(_startPosition, _endPosition, t);
            _rigidBody.MovePosition(newPos);
            if (t >= 1.0f)
            {
                _rigidBody.MovePosition(_endPosition);
                _isSpringActivated = false;
                StartCoroutine(ResetSpring());
            }
        }

        if (_isReturning)
        {
            _currentTime += Time.fixedDeltaTime;
            float t = _currentTime / time;

            Vector3 newPos = Vector3.Lerp(_endPosition, _startPosition, t);
            _rigidBody.MovePosition(newPos);

            if (t >= 1.0f)
            {
                _rigidBody.MovePosition(_startPosition);
                _isReturning = false;
                _isReady = true;
            }
        }
    }

    private IEnumerator ActivateSpring()
    {
        yield return new WaitForSeconds(delayTime);
        UnityEngine.Debug.Log("Spring is activated");
        _isSpringActivated = true;
        _currentTime = 0.0f;
    }

    private IEnumerator ResetSpring()
    {
        yield return new WaitForSeconds(resetDelay);
        _isReturning = true;
        _currentTime = 0.0f;
    }

    #endregion
}
