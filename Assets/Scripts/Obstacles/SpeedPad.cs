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
//        Debug.Log($"[{Time.time:F3}] Trigger enter: {other.name}");
        //Wheel�� �浹���� �� �ǽ��Ѵ�.
        /*
        if (other.gameObject.CompareTag("Wheel"))
        {
            UnityEngine.Debug.Log("Speed Pad Touched");
            Rigidbody carRigidbody = FindRigidbody(other.transform);
            Boost(carRigidbody, other.transform);
        }
        */



        if (!other.gameObject.CompareTag("Wheel")) return;

        Rigidbody carRb = FindRigidbody(other.transform);
        if (carRb == null) return;

        if (wheelHitDict.TryGetValue(carRb, out WheelHitInfo info))
        {
            // �̹� ���� ������ ���� ��, �ݴ��� ������ ����� ���
            if (!info.bothWheelsTouched)
            {
                info.bothWheelsTouched = true;
                if (info.timerCoroutine != null)
                    StopCoroutine(info.timerCoroutine);

                //Quaternion targetRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
                //carRb.MoveRotation(targetRotation);
                Vector3 padForward = transform.forward;
                Vector3 carForward = carRb.transform.forward;

                float dot = Vector3.Dot(carForward, padForward);

                // ���� ���� �� �״�� ȸ��
                // �ݴ� ���� �� ����� ȸ��
                Vector3 desiredDirection = dot >= 0 ? padForward : -padForward;

                Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
                carRb.MoveRotation(targetRotation);

                // �������� ���� ����
                //carRb.AddForce(transform.forward * boost, ForceMode.Acceleration);
                carRb.AddForce(transform.forward * boost, ForceMode.VelocityChange);
    //            UnityEngine.Debug.Log("Both Wheels Touched: Full Boost");
                wheelHitDict.Remove(carRb);
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

        // �ð� ���� �ݴ��� ������ �� ����� ���
        if (!info.bothWheelsTouched)
        {
            //rb.AddForceAtPosition(transform.forward * boost, info.firstWheel.position, ForceMode.Acceleration);
            //rb.AddForceAtPosition(transform.forward * boost, info.firstWheel.position, ForceMode.VelocityChange);
            rb.AddForce(transform.forward * (boost * 2 / 3), ForceMode.VelocityChange);

            // ȸ�� ���� �Ǵ� (���� ������ �������� ȸ��)
            Vector3 localWheelPos = rb.transform.InverseTransformPoint(info.firstWheel.position);

            if (localWheelPos.x < 0)
            {
                // ���� ���� �� �������� ȸ��
                rb.AddTorque(Vector3.up * torqueStrength, ForceMode.VelocityChange);
            }
            else
            {
                // ������ ���� �� ���������� ȸ��
                rb.AddTorque(Vector3.up * -torqueStrength, ForceMode.VelocityChange);
            }

            UnityEngine.Debug.Log("Single Wheel Boost Applied");
        }

        wheelHitDict.Remove(rb);
    }

    private Rigidbody FindRigidbody(Transform findingRb)
    {
        //�ö󰡴� ������ �ٵ� ã���� ���� ���� ���̴�.
        Rigidbody rb = null;

        while (findingRb != null)
        {
            //���� ��� �ִ� ������Ʈ�� ������ �ٰ� �ִ��� ã�ƺ���.
            rb = findingRb.GetComponent<Rigidbody>();
            //ã������ �� ������ �ʿ䰡 ������ break.
            if (rb != null)
            {
                break;
            }
            //�� ã������ �θ�� �ö󰣴�. ������ �θ����׼� ã�ƺ� ���̴�.
            findingRb = findingRb.parent;
        }

        UnityEngine.Debug.Log("Speed Pad finds RB");
        return rb;
    }

    /*
    private void Boost(Rigidbody rb, Transform wheel)
    {
        //���ǵ� �е��� �������� ������ ���� �ش�.
        rb.AddForceAtPosition(transform.forward * boost, wheel.position, ForceMode.Acceleration);
        UnityEngine.Debug.Log("Speed Pad Boost");
    }
    */

    #endregion
}
