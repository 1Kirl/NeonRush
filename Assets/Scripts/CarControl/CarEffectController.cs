using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shared.Protocol;

public class CarEffectController : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private Transform driftChargingRoot;
    [SerializeField] private Transform readyToBoostRoot;
    [SerializeField] private Transform driftBoostRoot;
    [SerializeField] private Transform deathRoot;
    [SerializeField] private Transform carTrailRoot;
    [SerializeField] private TrailRenderer carTrail;
    [SerializeField] public bool isTrailDefault = true;
    #endregion

    #region Private Variables
    [SerializeField] private List<ParticleSystem> driftCharging = new List<ParticleSystem>();
    [SerializeField] private List<AudioSource> driftChargingAudio = new List<AudioSource>();

    [SerializeField] private List<ParticleSystem> readyToBoost = new List<ParticleSystem>();
    [SerializeField] private List<AudioSource> readyToBoostAudio = new List<AudioSource>();


    [SerializeField] private List<ParticleSystem> driftBoost = new List<ParticleSystem>();
    [SerializeField] private List<AudioSource> driftBoostAudio = new List<AudioSource>();
    [SerializeField] private List<ParticleSystem> death = new List<ParticleSystem>();
    [SerializeField] private List<AudioSource> deathAudio = new List<AudioSource>();
    [SerializeField] private List<ParticleSystem> carTrails_not_default = new List<ParticleSystem>();
    #endregion

    public void BindingEffects()
    {
        driftCharging.Clear();
        readyToBoost.Clear();
        driftBoost.Clear();
        death.Clear();
        carTrails_not_default.Clear();

        if (driftChargingRoot != null)
        {
            driftCharging.AddRange(driftChargingRoot.GetComponentsInChildren<ParticleSystem>());
            driftChargingAudio.AddRange(driftChargingRoot.GetComponentsInChildren<AudioSource>());
        }

        if (readyToBoostRoot != null)
        {
            readyToBoost.AddRange(readyToBoostRoot.GetComponentsInChildren<ParticleSystem>());
            readyToBoostAudio.AddRange(readyToBoostRoot.GetComponentsInChildren<AudioSource>());
        }

        if (driftBoostRoot != null)
        {
            driftBoost.AddRange(driftBoostRoot.GetComponentsInChildren<ParticleSystem>());
            driftBoostAudio.AddRange(driftBoostRoot.GetComponentsInChildren<AudioSource>());
        }

        if (deathRoot != null)
        {
            death.AddRange(deathRoot.GetComponentsInChildren<ParticleSystem>());
            deathAudio.AddRange(deathRoot.GetComponentsInChildren<AudioSource>());
        }

        if (!isTrailDefault && carTrailRoot != null)
        {
            carTrails_not_default.AddRange(carTrailRoot.GetComponentsInChildren<ParticleSystem>());

        }
    }

    #region Public Methods
    private void StopEffect(List<ParticleSystem> particleList)
    {
        foreach (var ps in particleList)
        {
            if (ps != null)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    public void StopDriftChargeAndReady()
    {
        if (LiteNetLibManager.Instance.connectionStart)
            LiteNetLibManager.Instance.SendEffectSignal(EffectType.StopDriftChargeAndReady);

        StopEffect(driftCharging);
        StopEffect(readyToBoost);
    }

    public void PlayCharging()
    {
        if (LiteNetLibManager.Instance.connectionStart)
            LiteNetLibManager.Instance.SendEffectSignal(EffectType.Charging);
        StopEffect(driftCharging);

        foreach (var ps in driftCharging)
        {
            if (ps != null) ps.Play();
        }
        foreach (var ps in driftChargingAudio)
        {
            if (ps != null) ps.Play();
        }
    }

    public void PlayReady()
    {
        if (LiteNetLibManager.Instance.connectionStart)
            LiteNetLibManager.Instance.SendEffectSignal(EffectType.Ready);
        StopEffect(driftCharging);
        StopEffect(readyToBoost);

        foreach (var ps in readyToBoost)
        {
            if (ps != null) ps.Play();
        }
        foreach (var ps in readyToBoostAudio)
        {
            if (ps != null) ps.Play();
        }
    }

    public void PlayDriftBoost()
    {
        if (LiteNetLibManager.Instance.connectionStart)
        {
            LiteNetLibManager.Instance.SendEffectSignal(EffectType.DriftBoost);
        }
        StopEffect(driftCharging);
        StopEffect(readyToBoost);
        StopEffect(driftBoost);

        foreach (var ps in driftBoost)
        {
            if (ps != null) ps.Play();
        }
        foreach (var ps in driftBoostAudio)
        {
            if (ps != null) ps.Play();
        }
    }

    public void PlayDeath()
    {
        if (LiteNetLibManager.Instance.connectionStart)
            LiteNetLibManager.Instance.SendEffectSignal(EffectType.Death);

        foreach (var ps in death)
        {
            if (ps != null) ps.Play();
        }
        foreach (var ps in deathAudio)
        {
            if (ps != null)
            {
                ps.spatialBlend = 0f;
                ps.Play();
            }
        }
    }

    public void ResetTrail()
    {
        if (LiteNetLibManager.Instance.connectionStart)
            LiteNetLibManager.Instance.SendEffectSignal(EffectType.ResetTrail);
        Debug.Log("[CEC] Turn On Trail Called");
        if (isTrailDefault)
        {
            carTrail.Clear();
            carTrail.enabled = true;
        }
        else
        {
            foreach (var ps in carTrails_not_default)
            {
                if (ps != null) ps.Play();
            }
            Debug.Log("[CEC] Turnt On");
        }
    }

    public void TurnOffTrail()
    {
        if (LiteNetLibManager.Instance.connectionStart)
            LiteNetLibManager.Instance.SendEffectSignal(EffectType.TurnOffTrail);
        Debug.Log("[CEC] Turn Off Trail Called");

        if (isTrailDefault)
        {
            carTrail.enabled = false;
            carTrail.Clear();
        }
        else
        {
            foreach (var ps in carTrails_not_default)
            {
                if (ps != null)
                {
                    ps.Stop();
                    ps.Clear();
                }
                Debug.Log("[CEC] Turnt Off");

            }
        }
    }
    #endregion
}
