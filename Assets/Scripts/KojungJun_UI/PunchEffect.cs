using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PunchEffect : MonoBehaviour
{
    [Header("Punch 설정")]
    [SerializeField] private float punchScale = 1.2f;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private int vibrato = 3;
    [SerializeField] private float elasticity = 0.5f;

    private void Start() {
        // 이미지 또는 전체 Transform에 punch 적용
        transform.DOPunchScale(Vector3.one * (punchScale - 1f), duration, vibrato, elasticity)
                 .SetEase(Ease.OutBack);
    }
}
