using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.InputSystem;

public class FinishCountdownManager : MonoBehaviour
{
    public static FinishCountdownManager Instance;

    [SerializeField] private GameObject[] numberObjects; // 0~9 
    [SerializeField] private GameObject startImage;
    [SerializeField] private GameObject gameOverText;    

    private void Awake() {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    public IEnumerator StartCountdownSequence(float delayBeforeStart) {
        yield return StartCoroutine(WaitRealSeconds(delayBeforeStart)); 

        for (int i = 9; i >= 0; i--) {
            ShowNumber(i);
            SoundManager.Instance.PlaySFX(SFXType.EndCountDown);
            yield return StartCoroutine(WaitRealSeconds(1f));
        }
        SoundManager.Instance.PlaySFX(SFXType.RaceFinish);
        LiteNetLibManager.Instance.carTransform.GetComponent<PlayerInput>().enabled = false;
        LiteNetLibManager.Instance.carTransform.GetComponent<CarEffectController>().StopDriftChargeAndReady();
        LiteNetLibManager.Instance.carTransform.GetComponent<Car1Controller>().ApplyBrakeForFinishInMulti();
        LiteNetLibManager.Instance.carTransform.GetComponent<CarDieCheck>().isGameStarted = false;
        LiteNetLibManager.Instance.carTransform.GetComponent<Rigidbody>().isKinematic = true;
        ShowGameOverText();
    }

    private void ShowNumber(int number) {
        for (int i = 0; i < numberObjects.Length; i++)
            numberObjects[i].SetActive(false);

        var obj = numberObjects[number];
        obj.transform.localScale = Vector3.zero;
        obj.SetActive(true);
        Debug.Log($"[FinishCountdownManager] ShowNumber: {number} Ȱ��ȭ");
        obj.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
    }


    private void ShowGameOverText() {
        foreach (var obj in numberObjects)
            obj.SetActive(false);

        gameOverText.SetActive(true);
        gameOverText.transform.localScale = Vector3.zero;
        gameOverText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutElastic);

        StartCoroutine(CallGameOverUIAfterDelay());
    }

    private IEnumerator CallGameOverUIAfterDelay() {
        yield return StartCoroutine(WaitRealSeconds(1f)); 
        StartCoroutine(GameoverUIManager.Instance.StartGameoverSequence());
    }

    public IEnumerator StartStartCountdown() {
        for (int i = 3; i >= 1; i--) {
            ShowNumber(i);
            SoundManager.Instance.PlaySFX(SFXType.StartCountDown);
            yield return StartCoroutine(WaitRealSeconds(1f));
        }

        // 숫자 비활성화
        foreach (var obj in numberObjects)
            obj.SetActive(false);

        startImage.SetActive(true);
        startImage.transform.localScale = Vector3.zero;
        startImage.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);

        yield return StartCoroutine(WaitRealSeconds(1f));
        startImage.SetActive(false);
    }

    IEnumerator WaitRealSeconds(float seconds) {
        float start = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - start < seconds)
            yield return null;
    }
}
