using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundBinder : MonoBehaviour
{

    void Awake() {
        Button[] allButtons = FindObjectsOfType<Button>(true); // 비활성 포함
        foreach (Button btn in allButtons) {
            btn.onClick.AddListener(() => PlayClickSound());
        }
    }

    void PlayClickSound() {
        SoundManager.Instance.PlaySFX(SFXType.ClickButton);
    }
}
