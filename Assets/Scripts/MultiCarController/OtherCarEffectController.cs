using System.Collections;
using System.Collections.Generic;
using Shared.Protocol;
using UnityEngine;

public class OtherCarEffectController : MonoBehaviour
{
    #region SerializeFied private variables
    [SerializeField] private int _clientId = 99;
    [SerializeField] private Transform driftChargingRoot;

    [SerializeField] private Transform readyToBoostRoot; //Ready

    [SerializeField] private Transform driftBoostRoot; //DriftBoost
    [SerializeField] private Transform deathRoot; //DriftBoost
    [SerializeField] private Transform carTrailRoot;

    [SerializeField] private TrailRenderer carTrail;
    [SerializeField] public bool isTrailDefault = true;
    #endregion



    #region Private variables
    private ParticleSystem[] driftCharging; //Charging
    private ParticleSystem[] readyToBoost; //Ready
    private ParticleSystem[] driftBoost; //DriftBoost
    private ParticleSystem[] death; //Death
    private ParticleSystem[] carTrails_not_default;

    #endregion

    public int ClientId
    {
        get => _clientId;
        set
        {
            Debug.Log($"[EC] EffectController and Client Id Binded. Id: {value}");
            _clientId = value;
            LiteNetLibManager.Instance.clientsDic[value].OnEffectActive += PlayEffect;
        }
    }
    public void BindingEffects()
    {
        driftCharging = driftChargingRoot.GetComponentsInChildren<ParticleSystem>();
        readyToBoost = readyToBoostRoot.GetComponentsInChildren<ParticleSystem>();
        driftBoost = driftBoostRoot.GetComponentsInChildren<ParticleSystem>();
        death = deathRoot.GetComponentsInChildren<ParticleSystem>();
        if (!isTrailDefault)
        {
            carTrails_not_default = carTrailRoot.GetComponentsInChildren<ParticleSystem>();
        }
    }
    private void PlayEffect(EffectType effectType)
    {
        switch (effectType)
        {
            case EffectType.Death:
                PlayDeath();
                break;
            case EffectType.DriftBoost:
                PlayDriftBoost();
                break;
            case EffectType.Ready:
                PlayReady();
                break;
            case EffectType.Charging:
                PlayCharging();
                break;
            case EffectType.StopDriftChargeAndReady:
                StopDriftChargeAndReady();
                break;
            case EffectType.ResetTrail:
                PlayDeath();
                break;
            case EffectType.TurnOffTrail:
                PlayDeath();
                break;
            default:
                break;
        }
    }
    public void ResteTrail()
    {
        if (isTrailDefault)
        {
            carTrail.Clear();
            carTrail.enabled = true;
        }
        else
        {
            foreach (var particle in carTrails_not_default)
            {
                particle.Play();
            }
        }
    }
    public void TurnOffTrail()
    {
        if (isTrailDefault)
        {
            carTrail.enabled = false;
            carTrail.Clear();
        }
        else
        {
            foreach (var particle in carTrails_not_default)
            {
                particle.Stop();
                particle.Clear();
            }
        }
    }
    public void PlayDeath()
    {
        foreach (var particle in death)
        {
            particle.Play();
        }
    }
    public void PlayDriftBoost()
    {
        Debug.Log("PlayDriftBoost Called");
        StopEffect(driftCharging);
        StopEffect(readyToBoost);
        StopEffect(driftBoost);
        foreach (var particle in driftBoost)
        {
            particle.Play();
        }
    }
    public void PlayReady()
    {
        Debug.Log("PlayReady Called");
        StopEffect(driftCharging);
        StopEffect(readyToBoost);
        foreach (var particle in readyToBoost)
        {
            particle.Play();
        }
    }
    public void StopEffect(ParticleSystem[] particle)
    {
        foreach (var childPs in particle)
        {
            childPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
    public void StopDriftChargeAndReady()
    {
        StopEffect(driftCharging);
        StopEffect(readyToBoost);
    }
    public void PlayCharging()
    {
        Debug.Log("PlayCharing Called");
        StopEffect(driftCharging);
        foreach (var particle in driftCharging)
        {
            particle.Play();
        }
    }
}
