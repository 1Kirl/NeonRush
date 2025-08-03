using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkidMarkObj : MonoBehaviour
{
    public TrailRenderer leftTrail;
    public TrailRenderer rightTrail;

    private void Awake()
    {
        if (leftTrail == null)
            leftTrail = transform.Find("SkidMarkPointL").GetComponent<TrailRenderer>();

        if (rightTrail == null)
            rightTrail = transform.Find("SkidMarkPointR").GetComponent<TrailRenderer>();
    }

    public void ResetTrails()
    {
        leftTrail.Clear();
        rightTrail.Clear();
    }

    public void EnableTrails(bool enabled)
    {
        leftTrail.enabled = enabled;
        rightTrail.enabled = enabled;
    }
}
