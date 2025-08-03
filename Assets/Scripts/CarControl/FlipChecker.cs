/*using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FlipChecker : MonoBehaviour
{
    #region private variable

    private Car1Controller _car;

    private Rigidbody _carRb;

    private Quaternion _prevRotation;

    private Vector3 _rotationAccum;

    private bool _wasGrounded = true;

    private Vector3 prevForward, prevUp, prevRight;

    private TrickComboManager _comboManager;

    private const float ANGULAR_THRESHOLD = 10f; // 도, 무시할 수 있는 비지배 축 회전 기준
    #endregion






    #region SerializeField variable



    #endregion






    #region System.SerializableField
    [System.Serializable] public class IntEvent : UnityEvent<int> { }

    #endregion






    #region event
    public IntEvent onBackFlip;
    public IntEvent onFrontFlip;
    public IntEvent onLeftSpin;
    public IntEvent onRightSpin;
    public IntEvent onLeftRoll;
    public IntEvent onRightRoll;

    public UnityEvent onCarGrounded;
    public UnityEvent onCarAirborne; // add airbone

    #endregion





    #region private funcs

    private void Start()
    {
        UnityAction<int> increaseFlip = (flips) => {
            int current = PlayerPrefs.GetInt("flip_count", 0);
            current += flips;
            PlayerPrefs.SetInt("flip_count", current);
            PlayerPrefs.Save();

            Debug.Log($"[업적] flip_count += {flips}, 총: {current}");
            UpdateUserDataToServer("flip_count", current);
        };
        _carRb = GetComponent<Rigidbody>();
        _car = GetComponent<Car1Controller>();
        _comboManager = FindObjectOfType<TrickComboManager>();

        // Hook each UnityEvent
        onFrontFlip.AddListener(flips => _comboManager.AddTrick(TrickType.FrontFlip));
        onBackFlip.AddListener(flips => _comboManager.AddTrick(TrickType.BackFlip));
        onLeftRoll.AddListener(flips => _comboManager.AddTrick(TrickType.LeftRoll));
        onRightRoll.AddListener(flips => _comboManager.AddTrick(TrickType.RightRoll));
        onLeftSpin.AddListener(flips => _comboManager.AddTrick(TrickType.LeftSpin));
        onRightSpin.AddListener(flips => _comboManager.AddTrick(TrickType.RightSpin));
    }

    private void FixedUpdate()
    {

        if (!_car.IsGrounded())
        {
            if (_wasGrounded) {
                _prevRotation = _carRb.transform.rotation;
                _rotationAccum = Vector3.zero;
                _wasGrounded = false;
                onCarAirborne?.Invoke(); // Airbone
                UnityEngine.Debug.Log("Car Flying: set base rotation for flip checker");

                prevForward = _carRb.transform.forward;
                prevUp = _carRb.transform.up;
                prevRight = _carRb.transform.right;
            }
            else
            {
                Vector3 localAngularVelocity = transform.InverseTransformDirection(_carRb.angularVelocity);
                Vector3 deltaRotation = localAngularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime;


                // 가장 지배적인 축 하나만 누적
                Vector3 dominantDelta = GetDominantAxisRotation(deltaRotation);
                _rotationAccum += dominantDelta;

                CheckFlipWithAccumulation();
            }
        }
        else
        {

            if (!_wasGrounded)
            {
                UnityEngine.Debug.Log("Car Grounded: + Flip Score");
                _wasGrounded = true;
                _rotationAccum = Vector3.zero;
                onCarGrounded?.Invoke();
            }
        }
    }

    private Vector3 GetDominantAxisRotation(Vector3 delta)
    {
        float absX = Mathf.Abs(delta.x);
        float absY = Mathf.Abs(delta.y);
        float absZ = Mathf.Abs(delta.z);

        // 가장 큰 축만 남기고 나머지는 0
        if (absX >= absY && absX >= absZ && absY < ANGULAR_THRESHOLD && absZ < ANGULAR_THRESHOLD)
            return new Vector3(delta.x, 0f, 0f);
        else if (absY >= absX && absY >= absZ && absX < ANGULAR_THRESHOLD && absZ < ANGULAR_THRESHOLD)
            return new Vector3(0f, delta.y, 0f);
        else if (absZ >= absX && absZ >= absY && absX < ANGULAR_THRESHOLD && absY < ANGULAR_THRESHOLD)
            return new Vector3(0f, 0f, delta.z);

        // 지배 축 없음 → 회전 무시
        return Vector3.zero;
    }

    private void CheckFlipWithAccumulation()
    {
        if (Mathf.Abs(_rotationAccum.x) >= 360f)
        {
            int direction = _rotationAccum.x > 0 ? 1 : -1;
            int flips = Mathf.FloorToInt(Mathf.Abs(_rotationAccum.x) / 360f);
            if (direction > 0) onFrontFlip?.Invoke(flips);
            else onBackFlip?.Invoke(flips);
            Debug.Log($"[X] {(direction > 0 ? "Front Flip" : "Back Flip")} {flips} times");
            // 나머지 회전량만 남기고 다른 축은 0으로
            _rotationAccum = new Vector3(_rotationAccum.x - direction * flips * 360f, 0f, 0f);
        }
        else if (Mathf.Abs(_rotationAccum.y) >= 360f)
        {
            int direction = _rotationAccum.y > 0 ? 1 : -1;
            int flips = Mathf.FloorToInt(Mathf.Abs(_rotationAccum.y) / 360f);
            if (direction > 0) onRightSpin?.Invoke(flips);
            else onLeftSpin?.Invoke(flips);
            Debug.Log($"[Y] {(direction > 0 ? "Right Spin" : "Left Spin")} {flips} times");
            // 나머지 회전량만 남기고 다른 축은 0으로
            _rotationAccum = new Vector3(0f, _rotationAccum.y - direction * flips * 360f, 0f);
        }
        else if (Mathf.Abs(_rotationAccum.z) >= 360f)
        {
            int direction = _rotationAccum.z > 0 ? 1 : -1;
            int flips = Mathf.FloorToInt(Mathf.Abs(_rotationAccum.z) / 360f);
            if (direction > 0) onLeftRoll?.Invoke(flips);
            else onRightRoll?.Invoke(flips);
            Debug.Log($"[Z] {(direction > 0 ? "Left Roll" : "Right Roll")} {flips} times");
            // 나머지 회전량만 남기고 다른 축은 0으로
            _rotationAccum = new Vector3(0f, 0f, _rotationAccum.z - direction * flips * 360f);
        }
    }

    private void CheckFlipForAxis(ref float axisAccum, string axis) {
        if (Mathf.Abs(axisAccum) >= 360f) {
            int direction = axisAccum > 0f ? 1 : -1;
            int flips = Mathf.FloorToInt(Mathf.Abs(axisAccum) / 360f);
            axisAccum -= 360f * direction * flips;

            string trickName = "";

            switch (axis) {
                case "X":
                    trickName = direction > 0 ? "Front Flip" : "Back Flip";
                    if (direction > 0) onFrontFlip?.Invoke(flips);
                    else onBackFlip?.Invoke(flips);
                    break;

                case "Y":
                    trickName = direction > 0 ? "Right Spin" : "Left Spin";
                    if (direction > 0) onRightSpin?.Invoke(flips);
                    else onLeftSpin?.Invoke(flips);
                    break;

                case "Z":
                    trickName = direction > 0 ? "Left Roll" : "Right Roll";
                    if (direction > 0) onLeftRoll?.Invoke(flips);
                    else onRightRoll?.Invoke(flips);
                    break;
            }            
            Debug.Log($"[{trickName}] detected! Flips: {flips}");
        }
    }

    private float NormalizeAngle(float angle)
    {
        return (angle > 180f) ? angle - 360f : angle;
    }

    private void UpdateUserDataToServer(string key, int value) {
        Param param = new Param();
        param.Add(key, value);

        Where where = new Where();
        where.Equal("owner_inDate", BackEnd.Backend.UserInDate);

        BackEnd.Backend.GameData.Update("user_data", where, param, callback =>
        {
            if (callback.IsSuccess())
                Debug.Log($"[서버] {key} 서버 업데이트 완료: {value}");
            else
                Debug.LogWarning($"[서버] {key} 서버 업데이트 실패: {callback}");
        });
    }
    #endregion
}*/

using BackEnd;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FlipChecker : MonoBehaviour
{
    #region private variable

    private Car1Controller _car;
    private Rigidbody _carRb;
    private Quaternion _prevRotation;
    private Vector3 _rotationAccum;
    private bool _wasGrounded = true;
    private TrickComboManager _comboManager;

    private const float ANGULAR_THRESHOLD = 10f;

    #endregion

    #region SerializeFied private variables

    [SerializeField] private float additionalRotationThresholdX = 0.5f;
    [SerializeField] private float additionalRotationThresholdY = 0.1f;
    [SerializeField] private float additionalRotationThresholdZ = 0.1f;

    [SerializeField] private float additionalRotationXFront = 90.0f;
    [SerializeField] private float additionalRotationXBack = 45.0f;
    [SerializeField] private float additionalRotationY = 90.0f;
    [SerializeField] private float additionalRotationZ = 90.0f;


    #endregion

    #region events

    [System.Serializable] public class IntEvent : UnityEvent<int> { }

    public IntEvent onBackFlip;
    public IntEvent onFrontFlip;
    public IntEvent onLeftSpin;
    public IntEvent onRightSpin;
    public IntEvent onLeftRoll;
    public IntEvent onRightRoll;

    public UnityEvent onCarGrounded;
    public UnityEvent onCarAirborne;

    #endregion

    private void Start() {
        _carRb = GetComponent<Rigidbody>();
        _car = GetComponent<Car1Controller>();
        _comboManager = FindObjectOfType<TrickComboManager>();

        // 서버 저장용 로컬 업적 증가 이벤트
        UnityAction<int> increaseFlip = (flips) => {
            int current = PlayerPrefs.GetInt("flip_count", 0);
            current += flips;
            PlayerPrefs.SetInt("flip_count", current);
            PlayerPrefs.Save();

            Debug.Log($"[업적] flip_count += {flips}, 총: {current}");
            UpdateUserDataToServer("flip_count", current);
        };

        // 콤보 시스템 연동 + 업적 저장
        onFrontFlip.AddListener(flips => { _comboManager.AddTrick(TrickType.FrontFlip); increaseFlip(flips); });
        onBackFlip.AddListener(flips => { _comboManager.AddTrick(TrickType.BackFlip); increaseFlip(flips); });
        onLeftRoll.AddListener(flips => { _comboManager.AddTrick(TrickType.LeftRoll); increaseFlip(flips); });
        onRightRoll.AddListener(flips => { _comboManager.AddTrick(TrickType.RightRoll); increaseFlip(flips); });
        onLeftSpin.AddListener(flips => { _comboManager.AddTrick(TrickType.LeftSpin); increaseFlip(flips); });
        onRightSpin.AddListener(flips => { _comboManager.AddTrick(TrickType.RightSpin); increaseFlip(flips); });
    }

    private void FixedUpdate() {
        if (!_car.IsGrounded()) {

            //Vector3 localAngVel = transform.InverseTransformDirection(_carRb.angularVelocity);

            if (_wasGrounded) {
                _prevRotation = _carRb.rotation;

                // 누적 회전량 보정: 
                // 현재의 로컬 기준 각속도 가져오기
                Vector3 localAngularVelocity = transform.InverseTransformDirection(_carRb.angularVelocity);
                _rotationAccum = Vector3.zero;


                // x축 보정
                // 프론트 플립 위주로. 프론트 플립 아닐 땐 일반 보정.
                if (localAngularVelocity.x > additionalRotationThresholdX)
                { // 임계값은 상황에 맞게 조정
                    _rotationAccum.x = additionalRotationXFront;
                }
                else
                {
                    _rotationAccum.x = - additionalRotationXBack;
                }

                // y축 보정
                if (localAngularVelocity.y > additionalRotationThresholdY)
                { // 임계값은 상황에 맞게 조정
                    _rotationAccum.y = additionalRotationY;
                }
                else if (localAngularVelocity.y < -additionalRotationThresholdY)
                {
                    _rotationAccum.y = -additionalRotationY;
                }
                // z축 보정
                if (localAngularVelocity.z > additionalRotationThresholdZ)
                { // 임계값은 상황에 맞게 조정
                    _rotationAccum.z = additionalRotationZ;
                }
                else if (localAngularVelocity.z < -additionalRotationThresholdZ)
                {
                    _rotationAccum.z = -additionalRotationZ;
                }

                _wasGrounded = false;
                onCarAirborne?.Invoke();


                Debug.Log($"FlipChecker - Angular Velocity (Local): X: {localAngularVelocity.x:F2}, Y: {localAngularVelocity.y:F2}, Z: {localAngularVelocity.z:F2}");
                Debug.Log($"FlipChecker - Additional Rotation: X: {_rotationAccum.x:F2}, Y: {_rotationAccum.y:F2}, Z: {_rotationAccum.z:F2}");
            }
            else {
                Vector3 localAngularVelocity = transform.InverseTransformDirection(_carRb.angularVelocity);
                Vector3 deltaRotation = localAngularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime;

                Vector3 dominantDelta = GetDominantAxisRotation(deltaRotation);
                _rotationAccum += dominantDelta;

                CheckFlipWithAccumulation();
            }
        }
        else {
            if (!_wasGrounded) {
                _wasGrounded = true;
                _rotationAccum = Vector3.zero;
                onCarGrounded?.Invoke();
            }
        }
    }

    private Vector3 GetDominantAxisRotation(Vector3 delta) {
        float absX = Mathf.Abs(delta.x);
        float absY = Mathf.Abs(delta.y);
        float absZ = Mathf.Abs(delta.z);

        if (absX >= absY && absX >= absZ && absY < ANGULAR_THRESHOLD && absZ < ANGULAR_THRESHOLD)
            return new Vector3(delta.x, 0f, 0f);
        else if (absY >= absX && absY >= absZ && absX < ANGULAR_THRESHOLD && absZ < ANGULAR_THRESHOLD)
            return new Vector3(0f, delta.y, 0f);
        else if (absZ >= absX && absZ >= absY && absX < ANGULAR_THRESHOLD && absY < ANGULAR_THRESHOLD)
            return new Vector3(0f, 0f, delta.z);

        return Vector3.zero;
    }

    private void CheckFlipWithAccumulation() {
        if (Mathf.Abs(_rotationAccum.x) >= 360f) {
            int direction = _rotationAccum.x > 0 ? 1 : -1;
            int flips = Mathf.FloorToInt(Mathf.Abs(_rotationAccum.x) / 360f);
            if (direction > 0) onFrontFlip?.Invoke(flips);
            else onBackFlip?.Invoke(flips);
            _rotationAccum = new Vector3(_rotationAccum.x - direction * flips * 360f, 0f, 0f);
        }
        else if (Mathf.Abs(_rotationAccum.y) >= 360f) {
            int direction = _rotationAccum.y > 0 ? 1 : -1;
            int flips = Mathf.FloorToInt(Mathf.Abs(_rotationAccum.y) / 360f);
            if (direction > 0) onRightSpin?.Invoke(flips);
            else onLeftSpin?.Invoke(flips);
            _rotationAccum = new Vector3(0f, _rotationAccum.y - direction * flips * 360f, 0f);
        }
        else if (Mathf.Abs(_rotationAccum.z) >= 360f) {
            int direction = _rotationAccum.z > 0 ? 1 : -1;
            int flips = Mathf.FloorToInt(Mathf.Abs(_rotationAccum.z) / 360f);
            if (direction > 0) onLeftRoll?.Invoke(flips);
            else onRightRoll?.Invoke(flips);
            _rotationAccum = new Vector3(0f, 0f, _rotationAccum.z - direction * flips * 360f);
        }
    }

    private void UpdateUserDataToServer(string key, int value) {
        Param param = new Param();
        param.Add(key, value);

        Where where = new Where();
        where.Equal("owner_inDate", Backend.UserInDate);

        Backend.GameData.Update("user_data", where, param, callback => {
            if (callback.IsSuccess())
                Debug.Log($"[서버] {key} 서버 업데이트 완료: {value}");
            else
                Debug.LogWarning($"[서버] {key} 서버 업데이트 실패: {callback}");
        });
    }
}

