using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringPad : MonoBehaviour
{
    #region serialize variable

    [SerializeField] private float endHeight = 10.0f;

    [SerializeField] private float movingSpeed = 5.0f;

    [SerializeField] private float delayTime = 1.0f;

    [SerializeField] private float resetDelay = 2.0f;

    #endregion





    #region private variable

    private Vector3 _startPosition;

    private Vector3 _endPosition;

    private Rigidbody _rigidBody;

    //private bool _isSpringActivated = false;

    private enum SpringState { Idle, GoingUp, WaitingAtTop, Returning }

    private SpringState _state = SpringState.Idle;

    #endregion





    #region private funcs

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _startPosition = transform.position;
        _endPosition = _startPosition + Vector3.up * endHeight;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && _state == SpringState.Idle)
        {
            UnityEngine.Debug.Log("SpringPad is triggered");
            StartCoroutine(ActivateSpring());
        }
    }

    private void FixedUpdate()
    {
        /*
        if (_isSpringActivated && _rigidBody.position != _endPosition)
        {
            Vector3 newPos = Vector3.MoveTowards(_rigidBody.position, _endPosition, movingSpeed * Time.fixedDeltaTime);
            _rigidBody.MovePosition(newPos);
        }
        */
        switch (_state)
        {
            case SpringState.GoingUp:
                MoveTowards(_endPosition, SpringState.WaitingAtTop);
                break;
            case SpringState.Returning:
                MoveTowards(_startPosition, SpringState.Idle);
                break;
        }
    }

    private IEnumerator ActivateSpring()
    {
        UnityEngine.Debug.Log("SpringPad triggered. Waiting...");
        yield return new WaitForSeconds(delayTime);
        UnityEngine.Debug.Log("Spring is activated");
        //_isSpringActivated = true;
        _state = SpringState.GoingUp;
    }

    private IEnumerator ResetSpring()
    {
        yield return new WaitForSeconds(resetDelay);
        UnityEngine.Debug.Log("Spring resetting...");
        _state = SpringState.Returning;
    }

    private void MoveTowards(Vector3 target, SpringState nextState)
    {
        Vector3 newPos = Vector3.MoveTowards(_rigidBody.position, target, movingSpeed * Time.fixedDeltaTime);
        _rigidBody.MovePosition(newPos);

        if (Vector3.Distance(_rigidBody.position, target) < 0.01f)
        {
            _rigidBody.MovePosition(target);
            _state = nextState;

            if (nextState == SpringState.WaitingAtTop)
                StartCoroutine(ResetSpring());
        }
    }

    #endregion
}
