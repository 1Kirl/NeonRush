using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameoverUIManager : MonoBehaviour
{
    public static GameoverUIManager Instance;

    [Header("Panels")]
    public GameObject scoreCanvas;
    public GameObject ongameCanvas;
    public GameObject aftergameCanvas;

    [Header("Animation")]
    public RectTransform gameoverPanelTransform;
    public float slideInY = 0f;
    public float slideOutY = 2000f;

    private void Awake() {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    public IEnumerator StartGameoverSequence() {
        // ���� UI �����
        scoreCanvas.SetActive(false);
        ongameCanvas.SetActive(false);
        aftergameCanvas.SetActive(true);
        // �г� �ʱ� ��ġ ���� �� ����
        gameoverPanelTransform.anchoredPosition = new Vector2(0, slideOutY);
        gameoverPanelTransform.DOAnchorPosY(slideInY, 1f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(10f); // ������ ���� �ð�

        // �г� ����
        gameoverPanelTransform.DOAnchorPosY(slideOutY, 1f).SetEase(Ease.InCubic);
        yield return new WaitForSeconds(1f); // �ִϸ��̼� ���

        // Scene Change
        MultiplayResetter.ResetAll();
        FadeManager.Instance.FadeOut();
        yield return new WaitForSeconds(1f);
        
        SceneManager.LoadScene("TrackMake_2_Ko_Kirl_my");
    }
}
