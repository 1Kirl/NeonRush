using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityBall : MonoBehaviour
{
    #region serialize variable

    [SerializeField] private float lifeTime = 2.0f;
    [SerializeField] private float reactivationDelay = 2.0f;
    [SerializeField] private List<ParticleSystem> enterEffect = new List<ParticleSystem>();
    [SerializeField] private List<ParticleSystem> exitEffect = new List<ParticleSystem>();
    [SerializeField] private List<ParticleSystem> idleEffect = new List<ParticleSystem>();
    [SerializeField] private Transform enterEffectRoot;
    [SerializeField] private Transform exitEffectRoot;
    [SerializeField] private Transform idleEffectRoot;
    [SerializeField] private Transform carTransform;
    [SerializeField] private SphereCollider sourceCollider; // 반지름을 참조할 콜라이더
    [SerializeField] private Transform effectRoot; 
    #endregion

    #region events
    public event Action<Transform> OnEnterEffect;
    public event Action<Transform> OnExitEffect;

    #endregion




    #region private variable

    private bool _isAlive = true;

    private Coroutine _reactivateCoroutine;

    #endregion





    #region properties

    public bool IsAlive
    {
        get
        {
            return _isAlive;
        }
        set
        {
            _isAlive = value;
        }
    }

    public float LifeTime
    {
        get
        {
            return lifeTime;
        }
    }

    #endregion





    #region private funcs
    void Start()
    {
        effectRoot.localScale = new Vector3(sourceCollider.radius, sourceCollider.radius, sourceCollider.radius);
        enterEffect.AddRange(enterEffectRoot.GetComponentsInChildren<ParticleSystem>());
        exitEffect.AddRange(exitEffectRoot.GetComponentsInChildren<ParticleSystem>());
        idleEffect.AddRange(idleEffectRoot.GetComponentsInChildren<ParticleSystem>());
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            UnityEngine.Debug.Log("Player left GravityBall zone: reactivating GravityBall...");
            
            PlayExitEffect();
            _isAlive = true;

            if (_reactivateCoroutine != null)
                StopCoroutine(_reactivateCoroutine);

            _reactivateCoroutine = StartCoroutine(ReactivateAfterDelay());
        }
    }

    private IEnumerator ReactivateAfterDelay()
    {
        yield return new WaitForSeconds(reactivationDelay);

        _isAlive = true;
        UnityEngine.Debug.Log("GravityBall is now reactivated.");
        _reactivateCoroutine = null;
    }

    #endregion
    public void StopEffect(List<ParticleSystem> particleList)
    {
        foreach (var ps in particleList)
        {
            if (ps != null)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
    public void PlayEnterEffect(Transform car)
    {
        carTransform = car;
        StopEffect(idleEffect);
        OnEnterEffect?.Invoke(carTransform); // Event To RopeController
        foreach (var ps in enterEffect)
        {
            if (ps != null) ps.Play();
        }
    }
    public void PlayExitEffect()
    {
        OnExitEffect?.Invoke(carTransform);
        StopEffect(enterEffect);
        foreach (var ps in exitEffect)
        {
            if (ps != null) ps.Play();
        }
        PlayIdleEffect();
    }
    public void PlayIdleEffect()
    {
        StopEffect(enterEffect);
        foreach (var ps in idleEffect)
        {
            if (ps != null) ps.Play();
        }
    }
}
