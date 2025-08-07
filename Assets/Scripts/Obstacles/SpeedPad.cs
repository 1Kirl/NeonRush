using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPad : MonoBehaviour
{
    #region serialize variable

    [SerializeField] float boost = 30;

    [SerializeField] float waitTime = 0.07f; // �ݴ� ������ ��ٸ� �ð�

    [SerializeField] float torqueStrength = 50f; // ȸ����ų ��

    #endregion


    private Dictionary<Rigidbody, WheelHitInfo> wheelHitDict = new();

    private class WheelHitInfo
    {
        public Transform firstWheel;
        public bool bothWheelsTouched;
        public Coroutine timerCoroutine;
    }


    #region private funcs

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Wheel")) return;

        Rigidbody carRb = FindRigidbody(other.transform);
        if (carRb == null) return;

        if (wheelHitDict.TryGetValue(carRb, out WheelHitInfo info))
        {
            if (!info.bothWheelsTouched)
            {
                info.bothWheelsTouched = true;
                if (info.timerCoroutine != null)
                    StopCoroutine(info.timerCoroutine);

                Vector3 padForward = transform.forward;
                Vector3 carForward = carRb.transform.forward;

                float dot = Vector3.Dot(carForward, padForward);
                Vector3 desiredDirection = dot >= 0 ? padForward : -padForward;

                Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
                carRb.MoveRotation(targetRotation);

                carRb.AddForce(transform.forward * boost, ForceMode.VelocityChange);
                wheelHitDict.Remove(carRb);
                UnityEngine.Debug.Log("Double Wheel Boost Applied");
            }
        }
        else
        {
            // ó�� ���� ����
            WheelHitInfo newInfo = new WheelHitInfo
            {
                firstWheel = other.transform,
                bothWheelsTouched = false
            };
            newInfo.timerCoroutine = StartCoroutine(WaitForOtherWheel(carRb, newInfo));
            wheelHitDict[carRb] = newInfo;
        }

    }

    private IEnumerator WaitForOtherWheel(Rigidbody rb, WheelHitInfo info)
    {
        yield return new WaitForSeconds(waitTime);

        if (!info.bothWheelsTouched)
        {
            rb.AddForce(transform.forward * (boost * 2 / 3), ForceMode.VelocityChange);

            Vector3 localWheelPos = rb.transform.InverseTransformPoint(info.firstWheel.position);

            if (localWheelPos.x < 0)
            {
                rb.AddTorque(Vector3.up * torqueStrength, ForceMode.VelocityChange);
            }
            else
            {
                rb.AddTorque(Vector3.up * -torqueStrength, ForceMode.VelocityChange);
            }

            UnityEngine.Debug.Log("Single Wheel Boost Applied");
        }

        wheelHitDict.Remove(rb);
    }

    private Rigidbody FindRigidbody(Transform findingRb)
    {
        Rigidbody rb = null;

        while (findingRb != null)
        {
            rb = findingRb.GetComponent<Rigidbody>();
            if (rb != null)
            {
                break;
            }
            findingRb = findingRb.parent;
        }

        UnityEngine.Debug.Log("Speed Pad finds RB");
        return rb;
    }
    #endregion
}
