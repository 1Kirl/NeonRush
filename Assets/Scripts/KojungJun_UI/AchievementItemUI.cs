using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class AchievementItemUI : MonoBehaviour
{
    public TMP_Text progressText;
    public Image progressBarFill;

    private int currentValue;
    private int targetValue;

    [Header("Reward")]
    public Button rewardButton;
    public GameObject rewardLockIcon;
    public int unlockItemIndex;

    public void Setup(int target) {
        targetValue = target;
        UpdateProgress(currentValue);
        LockReward();
    }

    public void UpdateProgress(int value) {
        currentValue = value;
        progressText.text = $"{currentValue}/{targetValue}";
        progressBarFill.fillAmount = Mathf.Clamp01((float)value / targetValue);
    }

    public void LockReward() {
        if (rewardLockIcon != null)
            rewardLockIcon.SetActive(true);

        if (rewardButton != null)
            rewardButton.interactable = false;
    }

    public void UnlockReward() {
        if (rewardLockIcon != null)
            rewardLockIcon.SetActive(false);

        if (rewardButton != null) {
            rewardButton.interactable = true;
            rewardButton.transform.DOKill();
            rewardButton.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 10, 0.5f).SetEase(Ease.OutBack);
        }
    }
    public void OnRewardButtonClick() {
        AchievementManager.Instance.UnlockItem(unlockItemIndex);
    }

}