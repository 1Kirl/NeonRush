using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TrickComboManager : MonoBehaviour
{
    [SerializeField] private float comboTimeWindow = 2.5f;
    [SerializeField] private TrickComboController _comboUI;

    private List<(TrickType type, float time)> _trickHistory = new();

    private readonly Dictionary<TrickType[], string> _comboRules = new(new TrickSequenceComparer())
    {
        { new[] { TrickType.BackFlip, TrickType.RightRoll }, "Great Flip!" },
        { new[] { TrickType.BackFlip, TrickType.BackFlip }, "Fantastic Back Flip!" },
        { new[] { TrickType.FrontFlip, TrickType.FrontFlip }, "Double Front Combo!" },
        { new[] { TrickType.BackFlip, TrickType.RightRoll, TrickType.FrontFlip }, "Triple Trick!" }
    };

    private void Awake() {
        if (_comboUI == null) {
            _comboUI = FindObjectOfType<TrickComboController>();
            if (_comboUI == null)
                Debug.LogWarning("[TrickComboManager] TrickComboController�� ã�� �� �����ϴ�. Combo UI�� ��µ��� �ʽ��ϴ�.");
        }
    }

    public void AddTrick(TrickType trick) {
        float now = Time.time;
        _trickHistory.Add((trick, now));

        // ������ ��� ����
        _trickHistory.RemoveAll(t => now - t.time > comboTimeWindow);

        foreach (var rule in _comboRules) {
            var pattern = rule.Key;
            if (_trickHistory.Count >= pattern.Length) {
                var recent = _trickHistory.TakeLast(pattern.Length).ToArray();

                // �ð� ���� üũ
                float timeSpan = recent.Last().time - recent.First().time;
                if (timeSpan > comboTimeWindow)
                    continue;

                // ���� ��
                bool matched = true;
                for (int i = 0; i < pattern.Length; i++) {
                    if (recent[i].type != pattern[i]) {
                        matched = false;
                        break;
                    }
                }

                if (matched) {
                    Debug.Log($"Combo Detected: {rule.Value}");
                    _comboUI?.ShowRandomComboText(rule.Value);
                    SoundManager.Instance.PlaySFX(SFXType.Trick); 
                    //Handheld.Vibrate();
                    _trickHistory.RemoveRange(_trickHistory.Count - pattern.Length, pattern.Length);
                    break;
                }

            }
        }
    }


    public void ClearTricks() {
        _trickHistory.Clear();
    }

    private class TrickSequenceComparer : IEqualityComparer<TrickType[]>
    {
        public bool Equals(TrickType[] x, TrickType[] y) => x.SequenceEqual(y);

        public int GetHashCode(TrickType[] obj) {
            unchecked {
                int hash = 17;
                foreach (var trick in obj)
                    hash = hash * 31 + trick.GetHashCode();
                return hash;
            }
        }
    }
}
