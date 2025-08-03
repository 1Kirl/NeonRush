using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeController : MonoBehaviour
{
    [SerializeField] private Transform car;

    [SerializeField] private Transform rope;
    [SerializeField] private Transform enterEffectRoot;
    [SerializeField] private Transform exitEffectRoot;
    [SerializeField] private bool _isConnected = false;

    [SerializeField] private List<ParticleSystem> enterEffect = new List<ParticleSystem>();
    [SerializeField] private List<ParticleSystem> exitEffect = new List<ParticleSystem>();

    [SerializeField] private GravityBall gravityBall;

    void Start()
    {
        gravityBall.OnEnterEffect += PlayEnterEffectAtCar;
        gravityBall.OnExitEffect += PlayExitEffectAtCar;
        enterEffect.AddRange(enterEffectRoot.GetComponentsInChildren<ParticleSystem>());
        exitEffect.AddRange(exitEffectRoot.GetComponentsInChildren<ParticleSystem>());
    }
    public void StopEffect(List<ParticleSystem> particleList)
    {
        foreach (var ps in particleList)
        {
            if (ps != null)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
    void FixedUpdate()
    {
        if (!_isConnected) return;

        Vector3 posA = car.position;
        Vector3 posB = this.transform.position;
        Vector3 direction = posB - posA;
        float distance = direction.magnitude;

        rope.position = (posA + posB) / 2f;
        rope.rotation = Quaternion.LookRotation(direction);

        // 부모의 부모 스케일 기준으로 로컬 스케일 보정
        Vector3 desiredWorldScale = new Vector3(0.3f, 0.3f, distance);

        Transform parentOfParent = rope.parent?.parent;
        Vector3 parentScale = parentOfParent != null ? parentOfParent.lossyScale : Vector3.one;
        enterEffectRoot.position = car.position;
        exitEffectRoot.position = car.position;
        rope.localScale = new Vector3(
            desiredWorldScale.x / parentScale.x,
            desiredWorldScale.y / parentScale.y,
            desiredWorldScale.z / parentScale.z
        );
    }

    void PlayEnterEffectAtCar(Transform carTransform)
    {
        rope.gameObject.SetActive(true);
        car = carTransform;
        _isConnected = true;
        enterEffectRoot.position = carTransform.position;
        foreach (var ps in enterEffect)
        {
            if (ps != null) ps.Play();
        }
    }
    void PlayExitEffectAtCar(Transform carTransform)
    {
        StopEffect(enterEffect);
        car = null;
        _isConnected = false;
        exitEffectRoot.position = carTransform.position;
        foreach (var ps in exitEffect)
        {
            if (ps != null) ps.Play();
        }
        rope.gameObject.SetActive(false);
    }
}
