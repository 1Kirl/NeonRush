using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using Shared.Bits;
using Shared.Protocol;
using DG.Tweening;
public class MultiUIManager : MonoBehaviour
{
    //[SerializeField] private GameObject WaitPopUp;
    [SerializeField] private TMP_Text PopUpText;
    [SerializeField] private List<Image> userIcons; // 8���� Image (��� ������) ��ü�� inspector���� ����
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.white;
    private RankingUIManager rankingUIManager;

    public void PopUpWait()
    {
        //WaitPopUp.SetActive(true);
        PopUpText.text = "Waiting other players...";
    }
    public void UpdateCurrentMembers(int totalMember)
    {
        UpdateUserIcons(totalMember);
        PopUpText.text = $"Waiting other players... [{totalMember}/8]";
    }  
    public void UpdateMatchFound()
    {
        PopUpText.text = "Match found! The game starts soon...";
    }
    public void UpdateRankingUI(List<RankingEntry> list) {
        rankingUIManager.UpdateRankingUI(list); // �ִϸ��̼� �� ���� ó�� ����
    }
    private void UpdateUserIcons(int count) {
        for (int i = 0; i < userIcons.Count; i++) {
            var icon = userIcons[i];
            bool shouldBeActive = i < count;

            if (shouldBeActive && icon.color != activeColor) {
                // �� ����
                icon.DOColor(activeColor, 0.2f);

                if (!DOTween.IsTweening(icon.transform)) {
                    icon.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 10, 1.0f)
                        .SetEase(Ease.OutQuad);
                }
            }
            else if (!shouldBeActive && icon.color != inactiveColor) {
                icon.DOColor(inactiveColor, 0.2f);
            }
        }
    }
}

