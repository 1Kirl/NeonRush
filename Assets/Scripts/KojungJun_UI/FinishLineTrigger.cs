using System.Collections;
using Shared.Network;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class FinishLineTrigger : MonoBehaviour
{
    private bool hasSent = false;
    [SerializeField] private CameraManager cameraManager;
    private void OnTriggerEnter(Collider other)
    {
        if (hasSent) return;

        if (other.CompareTag("Player"))
        {

            hasSent = true;

            //deactivate playerinput if it's me
            var input = other.GetComponentInParent<PlayerInput>()
                        ?? other.GetComponentInChildren<PlayerInput>();
            if (input != null)
            {
                StartCoroutine(WaitForCarToStopThenMoveCam(other.gameObject.transform));
                input.enabled = false;
                other.gameObject.GetComponent<CarDieCheck>().isGameStarted = false;
            }
            other.gameObject.GetComponent<CarEffectController>().StopDriftChargeAndReady();
            other.gameObject.GetComponent<Car1Controller>().ApplyBrakeForFinishInMulti();
            int currentScore = ScoreManager.Instance.CurrentScore;
            ClientMessageSender.SendReachedFinishLine((ushort)currentScore);

            Debug.Log($"[FinishLine] Player reached finish line. Score: {currentScore}");
        }
    }
    private IEnumerator WaitForCarToStopThenMoveCam(Transform target)
    {
        Car1Controller car = target.GetComponent<Car1Controller>();
        if (car == null)
            yield break;

        // 0에 가까운 속도일 때까지 대기
        while (car.CurrentSpeed > 30f) // 혹은 Mathf.Abs(car.CurrentSpeed) > 0.1f
        {
            yield return null;
        }
        target.GetComponent<Rigidbody>().isKinematic = true;
        cameraManager.FinishCamMove(target);
    }
}
