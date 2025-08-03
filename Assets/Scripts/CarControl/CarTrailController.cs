using System.Collections;
using System.Collections.Generic;
using Shared.Protocol;
using UnityEngine;

public class CarTrailController : MonoBehaviour
{
    public TrailRenderer carTrail;
    void Start()
    {
        if (carTrail != null)
        {
            carTrail.enabled = true;
        }
    }
    public void ResetTrail()
    {
        LiteNetLibManager.Instance.SendEffectSignal(EffectType.ResetTrail);
        carTrail.Clear();
        carTrail.enabled = true;
    }
    public void TurnOffTrail()
    {
        LiteNetLibManager.Instance.SendEffectSignal(EffectType.TurnOffTrail);
        carTrail.enabled = false;
    }
}
