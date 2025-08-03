using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    #region serialize variable

    [SerializeField] private Transform[] points;

    [SerializeField] private float[] times;

    [SerializeField] private float[] delayTimes;

    [SerializeField] private bool startDelay = false;

    [SerializeField] private float startDelayTime = 1.0f;

    [SerializeField] private bool moving = false;

    [SerializeField] private bool rotating = false;

    #endregion





    #region private variable

    private Rigidbody _rigidBody;

    private float _curruntTime = 0;

    private Transform _currentTransform;

    private Transform _nextTransform;

    private bool _isActivated = true;

    private bool _isDelaying = false;

    private int _index = 0;

    #endregion





    #region private funcs

    private void Start()
    {
        _rigidBody = this.GetComponent<Rigidbody>();
        ChooseTransforms();

        if (startDelay)
        {
            UnityEngine.Debug.Log("StartDelay is True");
            _isActivated = false;
            StartCoroutine(StartDelay());
        }
        else
        {
            UnityEngine.Debug.Log("StartDelay is False");
        }
    }

    private IEnumerator StartDelay()
    {
        _isDelaying = true;
        yield return new WaitForSeconds(startDelayTime);
        _isActivated = true;
        _isDelaying = false;
    }

    private void ChooseTransforms()
    {
        _currentTransform = points[_index];
        _nextTransform = points[(_index + 1) % points.Length];
        //UnityEngine.Debug.Log("currentTransform.position: " + currentTransform.position);
        //UnityEngine.Debug.Log("nextTransform.position: " + nextTransform.position);
    }

    private void FixedUpdate()
    {
        if (_isActivated)
        {
            Work();
        }
        else if (!_isDelaying)
        {
            StartCoroutine(Delay());
        }
    }

    // 활성화중이면, 발판이 얼마나 동작해야 하는지 계산해서 넘겨준다.
    private void Work()
    {
        _curruntTime += Time.fixedDeltaTime;
        float t = _curruntTime / times[_index];

        if (moving)
        {
            Move(t);
        }
        if (rotating)
        {
            Rotate(t);
        }

        if (t >= 1)
        {
            _isActivated = false;
        }
    }

    // 발판을 움직이는 것에만 집중한다.
    private void Move(float t)
    {
        Vector3 newPos = Vector3.Lerp(_currentTransform.position, _nextTransform.position, t);
        _rigidBody.MovePosition(newPos);
        if (t >= 1.0f)
        {
            _rigidBody.MovePosition(_nextTransform.position);
        }
    }

    //발판을 회전하는 것에만 집중한다.
    private void Rotate(float t)
    {
        Quaternion newRotation = Quaternion.Slerp(_currentTransform.rotation, _nextTransform.rotation, t);
        _rigidBody.MoveRotation(newRotation);
        if (t >= 1.0f)
        {
            _rigidBody.MoveRotation(_nextTransform.rotation);
        }
    }

    private IEnumerator Delay()
    {
        _isDelaying = true;
        yield return new WaitForSeconds(delayTimes[_index]);
        _isActivated = true;
        _curruntTime = 0.0f;
        _index = (_index + 1) % points.Length;
        ChooseTransforms();
        _isDelaying = false;
    }

    #endregion
}
