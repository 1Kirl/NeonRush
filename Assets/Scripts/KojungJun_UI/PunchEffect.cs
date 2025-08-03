using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PunchEffect : MonoBehaviour
{
    [Header("Punch ����")]
    [SerializeField] private float punchScale = 1.2f;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private int vibrato = 3;
    [SerializeField] private float elasticity = 0.5f;

    private void Start() {
        // �̹��� �Ǵ� ��ü Transform�� punch ����
        transform.DOPunchScale(Vector3.one * (punchScale - 1f), duration, vibrato, elasticity)
                 .SetEase(Ease.OutBack);
    }
}
