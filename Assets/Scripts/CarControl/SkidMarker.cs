using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SkidMarker : MonoBehaviour
{
    [SerializeField] private Car1Controller car1Controller;

    [SerializeField] private SkidMarkPool skidMarkPool;

    private GameObject currentSkidMark;
    private SkidMarkObj skidMarkObj;
    [SerializeField]private Transform skidMarkPosOfCar;
    [SerializeField] private WheelCollider rearWheelL;
    [SerializeField] private WheelCollider rearWheelR;
    void Start()
    {
        car1Controller.OnDriftSkidMark += MakeSkidMark;
    }
    public void InitSkidMarker()
    {
        skidMarkPool = GameObject.Find("PoolManager").GetComponent<SkidMarkPool>();
    }
    private float findNormalOfGround()
    {
        WheelHit hit;

        if (rearWheelL.GetGroundHit(out hit))
        {
            Vector3 normal = hit.normal;

            // 노멀 벡터를 이용한 회전 계산
            Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, normal);
            float groundZRotation = surfaceRotation.eulerAngles.z;

            Debug.Log("지면의 Z 회전: " + groundZRotation);
            return groundZRotation;
        }

        if (rearWheelR.GetGroundHit(out hit))
        {
            Vector3 normal = hit.normal;
            // 노멀 벡터를 이용한 회전 계산
            Quaternion surfaceRotation = Quaternion.FromToRotation(Vector3.up, normal);
            float groundZRotation = surfaceRotation.eulerAngles.z;

            Debug.Log("지면의 Z 회전: " + groundZRotation);
            return groundZRotation;
        }

        return skidMarkPosOfCar.rotation.eulerAngles.z;
    }
    public void MakeSkidMark(bool activate)
    {
        if (activate)
        {
            Debug.Log("Drift starts - skidMarkers");
            if (currentSkidMark == null)
            {

                Debug.Log("theres no currentSkid");

                currentSkidMark = skidMarkPool.GetSkidMark();

                // stick to the car
                currentSkidMark.transform.SetParent(skidMarkPosOfCar);

                // set the exact position of trail to be the same as parent
                // set the rotation of trail to be the same as parent except for rotation of the z-axis
                // the rotation of the z-axis is extracted from the nomal vector of the ground where the wheel is on
                currentSkidMark.transform.localPosition = Vector3.zero;
                //Vector3 newEulerBasedOnGround = new Vector3(0f, 0f, findNormalOfGround());
                currentSkidMark.transform.localRotation = Quaternion.identity;
                //currentSkidMark.transform.rotation = Quaternion.Euler(newEulerBasedOnGround);

                skidMarkObj = currentSkidMark.GetComponent<SkidMarkObj>();
            }

            skidMarkObj.EnableTrails(true);
        }
        else
        {
            Debug.Log("Drift ENDs - skidMarkers");
            if (currentSkidMark != null)
            {
                // Remove the relationship with parents and stick it to the world
                currentSkidMark.transform.SetParent(null, true);
                // Return after a while
                StartCoroutine(ReturnToPoolAfterDelay(currentSkidMark, skidMarkObj, 1.5f));
                skidMarkObj = null;
                currentSkidMark = null;
            }
        }
    }

    private IEnumerator ReturnToPoolAfterDelay(GameObject skidMark, SkidMarkObj skidMarkObj, float delay)
    {
        yield return new WaitForSeconds(delay);
        // Deactivate TrailRenderer
        skidMarkObj.EnableTrails(false);
        skidMarkPool.ReturnSkidMark(skidMark);
    }
}