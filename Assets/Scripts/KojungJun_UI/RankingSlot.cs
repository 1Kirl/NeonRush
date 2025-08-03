using TMPro;
using UnityEngine;

public class RankingSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text rankingText;

    [HideInInspector] public ushort CurrentClientId;

    public void SetData(string name, int score, bool isMe) {
        rankingText.color = isMe ? Color.green : Color.white;
        rankingText.text = $"{name}: {score}";
    }

    public void Clear() {
        rankingText.text = "";
        CurrentClientId = 0;
    }

    public RectTransform GetTextTransform() {
        return rankingText.rectTransform;
    }
}