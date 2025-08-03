using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;
using System;

public class CarDieCheck : MonoBehaviour
{
    #region Public var
    public LayerMask trackLayer;
    public LayerMask obstacleLayer;
    public LayerMask deathHeightLayer;
    public GameObject canvas;
    public Collider DeathCollider1;
    public Collider DeathCollider2;
    public Collider DeathCollider3;
    public Collider DeathCollider4_For_Obstacle;
    public Collider wheelsSphereCollider_FL;
    public Collider wheelsSphereCollider_FR;
    public Collider wheelsSphereCollider_RL;
    public Collider wheelsSphereCollider_RR;
    public Rigidbody carRigidBody;
    public Collider[] carColliders;
    [SerializeField] public FlipScoreManager flipScoreManager;
    [SerializeField] public TrackManager trackManager;
    [SerializeField] public FlipText flipText;
    [SerializeField] public Transform[] reSpawnPoints;
    [SerializeField] private Renderer materialRenderer;
    [SerializeField] private Renderer[] wheelsMaterialRenderer;
    [SerializeField] private Color dieColor = new Color(1f, 1f, 1f)* 2.5f;
    [SerializeField] private Color originColor = new Color();
    [SerializeField] private Color wheelsOriginColor = new Color();
    private Material mat;
    private Material[] wheelsMat = new Material[4];
    #endregion

    public bool isMulti = false;
    public bool isGameStarted = false;

    #region event
    [SerializeField] public event Action OnResetCamToStart;
    [SerializeField] public event Action OnCamToStart;
    [SerializeField] public event Action OnDieCam;
    [SerializeField] public event Action OnResetFloor;
    [SerializeField] public event Action<bool, Transform> OnResetCarPos;
    [SerializeField] private UnityEvent OnSkidMarkerOff;
    [SerializeField] private UnityEvent OnResetCarTrail;
    [SerializeField] private UnityEvent OnTurnOffCarTrail;
    [SerializeField] private UnityEvent OnDieEffect;
    [SerializeField] private UnityEvent OnDisableCarControll;
    [SerializeField] private UnityEvent OnEnbleCarControll;
    [SerializeField] private float movementThreshold = 2f;       // 얼마나 움직여야 "움직였다"고 판단할지 (단위: m)
    [SerializeField] private float maxIdleTime = 5f;             // 움직이지 않으면 죽은 걸로 판단할 시간 (단위: 초)

    private Vector3 lastCheckedPosition;
    private float idleTimer = 0f;
    #endregion

    #region private Variables
    private bool collisionOccured = false;
    #endregion


    #region private funcs
    private void Start()
    {
        OnResetCamToStart += GameObject.Find("CameraManager").GetComponent<CameraManager>().ResetToStart;
        OnCamToStart += GameObject.Find("CameraManager").GetComponent<CameraManager>().GameStart;
        OnDieCam += GameObject.Find("CameraManager").GetComponent<CameraManager>().SwitchToDeathCam;
        OnResetCarPos += GetComponent<Car1Controller>().ResetCarPos;
        mat = materialRenderer.material;
        int i = 0;
        foreach (var wheelMaterialRenderer in wheelsMaterialRenderer)
        {
            wheelsMat[i] = wheelMaterialRenderer.material;
            i++;
        }
        originColor = mat.GetColor("_EmissionColor");
        wheelsOriginColor = wheelsMat[0].GetColor("_EmissionColor");
        lastCheckedPosition = transform.position;

    }
    void Update()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastCheckedPosition);

        if (isGameStarted == false)
        {
            lastCheckedPosition = transform.position;

            idleTimer = 0f;
            return;
        }

        if (distanceMoved < movementThreshold)
            {
                idleTimer += Time.deltaTime;
                if (idleTimer >= maxIdleTime)
                {
                    if (!ScoreManager.Instance.IsDead)
                    {
                        ScoreManager.Instance.StopTracking(isMulti); //_isDead = true
                    }

                    Debug.Log("[CarDieCheck] idle time is over! you dead");
                    OnDieEffect.Invoke();
                    OnSkidMarkerOff.Invoke();
                    OnDieCam.Invoke();
                    OnTurnOffCarTrail.Invoke();
                    OnDisableCarControll.Invoke(); //playerInput inactivate
                                                   //Freeze The Car and Allow Obstacles to penetrate it
                    DieFreeze();

                    //wait for secs
                    isGameStarted = false;
                    StartCoroutine(DeathDelay(isMulti));
                }
            }
            else
            {
                idleTimer = 0f;
                lastCheckedPosition = transform.position;
            }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Reset the car's position to start point
        // Reset the cam's priority the same as the start
        // Set the car's kinematic as false
        // Set the canvas visible  ->  it should be changed later as visualize the other sections
        // ------ MVP's set
        // Save the score & Reset it
        if (collisionOccured) return;

        Debug.Log("Collision to collider!");
        if (((1 << collision.gameObject.layer) & trackLayer) != 0)
        {
            Debug.Log("Collision to track!");
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.thisCollider == DeathCollider1
                || contact.thisCollider == DeathCollider2
                || contact.thisCollider == DeathCollider3)
                {
                    collisionOccured = true;
                    if (!ScoreManager.Instance.IsDead)
                    {
                        ScoreManager.Instance.StopTracking(isMulti); //_isDead = true
                    }

                    Debug.Log("Collision to deathCollider!");
                    OnDieEffect.Invoke();
                    OnSkidMarkerOff.Invoke();
                    OnDieCam.Invoke();
                    OnTurnOffCarTrail.Invoke();
                    OnDisableCarControll.Invoke(); //playerInput inactivate
                    //Freeze The Car and Allow Obstacles to penetrate it
                    DieFreeze();

                    //wait for secs
                    isGameStarted = false;
                    StartCoroutine(DeathDelay(isMulti));
                    break;
                }
            }
        }
        else if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            Debug.Log("Collision to obstacle!");
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.thisCollider == DeathCollider1
                || contact.thisCollider == DeathCollider2
                || contact.thisCollider == DeathCollider3
                || contact.thisCollider == DeathCollider4_For_Obstacle
                || contact.thisCollider == wheelsSphereCollider_FL
                || contact.thisCollider == wheelsSphereCollider_FR
                || contact.thisCollider == wheelsSphereCollider_RL
                || contact.thisCollider == wheelsSphereCollider_RR
                )
                {
                    collisionOccured = true;
                    if (!ScoreManager.Instance.IsDead)
                    {
                        ScoreManager.Instance.StopTracking(isMulti); //_isDead = true
                    }

                    Debug.Log("Collision to deathCollider!");
                    OnDieEffect.Invoke();
                    OnSkidMarkerOff.Invoke();
                    OnDieCam.Invoke();
                    OnTurnOffCarTrail.Invoke();
                    OnDisableCarControll.Invoke(); //playerInput inactivate
                    //Freeze The Car and Allow Obstacles to penetrate it
                    DieFreeze();

                    //wait for secs
                    isGameStarted = false;
                    StartCoroutine(DeathDelay(isMulti));
                    break;
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (collisionOccured) return;

        // DeathLayer에 해당하는 오브젝트인지 확인
        if (((1 << other.gameObject.layer) & deathHeightLayer) != 0)
        {
            collisionOccured = true;

            if (!ScoreManager.Instance.IsDead)
            {
                ScoreManager.Instance.StopTracking(isMulti); // _isDead = true
            }

            Debug.Log("Trigger Enter with deathCollider!");
            OnDieEffect.Invoke();
            OnSkidMarkerOff.Invoke();
            OnDieCam.Invoke();
            OnTurnOffCarTrail.Invoke();
            OnDisableCarControll.Invoke(); //playerInput inactivate

            DieFreeze();
            isGameStarted = false;
            StartCoroutine(DeathDelay(isMulti));
        }
    }

    #endregion

    private void DieFreeze()
    {
        if (materialRenderer != null)
        {
            StartCoroutine(BlinkEmission());
        }

        carRigidBody.isKinematic = true;
        foreach (var collider in carColliders)
        {
            collider.isTrigger = true;
        }
    }
    private void RestoreDieFreeze()
    {
        carRigidBody.isKinematic = false;
        foreach (var collider in carColliders)
        {
            collider.isTrigger = false;
        }
    }
    IEnumerator BlinkEmission()
    {
        int blinkCount = 4;
        float interval = 0.25f;

        for (int i = 0; i < blinkCount; i++)
        {
            mat.SetColor("_EmissionColor", dieColor);
            // blink the wheels as well
            foreach (var wheelMat in wheelsMat)
            {
                wheelMat.SetColor("_EmissionColor", dieColor);
            }
            yield return new WaitForSeconds(interval);

            mat.SetColor("_EmissionColor", originColor);
            foreach (var wheelMat in wheelsMat)
            {
                wheelMat.SetColor("_EmissionColor", wheelsOriginColor);
            }
            yield return new WaitForSeconds(interval);
        }
        mat.SetColor("_EmissionColor", originColor);
        foreach (var wheelMat in wheelsMat)
        {
            wheelMat.SetColor("_EmissionColor", wheelsOriginColor);
        }
    }
    private Transform GetClosestRespawnPointBehind(Transform[] respawnPoints, Transform selfTransform)
    {
        Transform closest = null;
        float closestDistance = float.MaxValue;

        foreach (Transform point in respawnPoints)
        {
            float deltaZ = point.position.z - selfTransform.position.z;

            // 자기보다 앞에 있으면 무시
            if (deltaZ >= 0)
                continue;

            float distance = Vector3.Distance(selfTransform.position, point.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = point;
            }
        }

        // 전부 자기보다 앞에 있으면 첫 번째 리스폰 포인트 반환
        if (closest == null && respawnPoints.Length > 0)
        {
            return respawnPoints[0];
        }

        return closest;
    }


    #region Coroutines
    private IEnumerator DeathDelay(bool isMulti)
    {
        flipText.ResetAll();
        flipScoreManager.ResetAllComboData();
        if (isMulti)
        {
            Debug.Log("[CarDieCheck] Deathdalay starts in multi");
            yield return new WaitForSeconds(2f);
            OnResetCarTrail.Invoke();
            RestoreDieFreeze();
            // Re-Position to ReStart Point
            OnResetCarPos.Invoke(isMulti, GetClosestRespawnPointBehind(reSpawnPoints, this.transform));
            OnCamToStart.Invoke(); //instead of GameStart
            isGameStarted = true; // if and only if it's multi -> game goes on
        }
        else
        {
            Debug.Log("[CarDieCheck] Deathdalay starts in single");
            yield return new WaitForSeconds(2f);
            OnResetFloor?.Invoke();
            //OnResetCarTrail.Invoke();
            RestoreDieFreeze();
            trackManager.ClearTracksOnGameOver();
            OnResetCarPos.Invoke(isMulti, null);
            canvas.SetActive(true);
            OnResetCamToStart.Invoke();
        }
        Debug.Log("[CarDieCheck] enable Car Control - playerInput");
        OnEnbleCarControll.Invoke();
        ScoreManager.Instance.ResetDeathStatus(); //_isDeat = false & _restartOnce = true
        collisionOccured = false;
    }
    #endregion

}
