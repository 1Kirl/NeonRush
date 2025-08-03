using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    #region serialize variable

    [SerializeField] private float directionThreshold = 0.8f;

    #endregion





    #region private variable

    private Rigidbody _rigidBody;

    private GravityBall _connectedBall;

    private Transform _ballTransform;

    private float _maxDistance;

    private bool _isConnected = false;

    private Coroutine _deactivateCoroutine;

    private float _lifeTime = 2;

    private Vector3 _direction;

    #endregion





    #region private funcs

    private void Start()
    {
        UnityEngine.Debug.Log("There is Rope!!.");
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("GravityBall"))
        {
            _connectedBall = other.gameObject.GetComponent<GravityBall>();

            if (!_isConnected && _connectedBall.IsAlive)
            {
                UnityEngine.Debug.Log("Rope Active.");
                ActivateRope(other.gameObject);
                _connectedBall.PlayEnterEffect(this.transform);
                this._lifeTime = _connectedBall.LifeTime;
                _deactivateCoroutine = StartCoroutine(LifeTimeOver());
            }
        }
    }

    private void ActivateRope(GameObject BallToConnect)
    {
        this._ballTransform = BallToConnect.transform;
        _maxDistance = Vector3.Distance(this.transform.position, _ballTransform.position);
        _isConnected = true;
    }

    private IEnumerator LifeTimeOver()
    {
        yield return new WaitForSeconds(_lifeTime);

        UnityEngine.Debug.Log("Rope Deactive: Life Time.");
        DeactiveRope();
    }

    private void DeactiveRope()
    {
        // �ڷ�ƾ ���� ���̾����� ���
        if (_deactivateCoroutine != null)
        {
            StopCoroutine(_deactivateCoroutine);
            _deactivateCoroutine = null;
        }

        _isConnected = false;
        _connectedBall.IsAlive = false;
        _connectedBall = null;
    }

    private void FixedUpdate()
    {
        if (_isConnected)
        {
            Work();
            CheckObstacle();
            CheckDirection();
        }
    }

    private void Work()
    {
        // �ڵ������� �������� ����� �Ÿ��� ���Ѵ�.
        // ������ ���ص״ٰ� ���� ���� ���� ���ǿ� ���δ�.
        _direction = this.transform.position - _ballTransform.position;
        float distance = _direction.magnitude;
        _direction.Normalize();


        // �������� �־������� �ӵ� ���� ����
        Vector3 velocity = _rigidBody.velocity;
        float dot = Vector3.Dot(velocity, _direction);
        if (dot > 0f)
        {
            Vector3 cancelVelocity = _direction * dot;
            _rigidBody.velocity -= cancelVelocity;
        }


        if (distance > _maxDistance)
        {
            Vector3 newPosition = _ballTransform.position + _direction * _maxDistance;

            _rigidBody.MovePosition(newPosition);
        }
        else if (distance < _maxDistance)
        {
            _maxDistance = distance;
        }
    }

    private void CheckObstacle()
    {
        if (Physics.Linecast(this.transform.position, _ballTransform.position, out RaycastHit hit))
        {

            UnityEngine.Debug.Log("Rope Deactivated: Obstacle detected.");

            DeactiveRope();
        }
    }

    private void CheckDirection()
    {

        if (Vector3.Dot(this.transform.up, _direction) > directionThreshold)
        {
            UnityEngine.Debug.Log("Rope Deactivated: Direction detected.");

            DeactiveRope();
        }
    }

    #endregion
}
