using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class UIButtonInputReceiver : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Local_InputReceiver inputReceiver;
    [SerializeField] private bool isLeft;
    [SerializeField] private bool isAcc;
    private void OnEnable()
    {
        // SetActive(true) 될 때마다 Player를 찾음
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("Player 태그가 붙은 오브젝트를 찾지 못했습니다.");
            return;
        }

        inputReceiver = player.GetComponent<Local_InputReceiver>();

        if (inputReceiver == null)
        {
            Debug.LogWarning("Local_InputReceiver 컴포넌트가 없습니다.");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.LogWarning("Button Down!");
        if (inputReceiver != null)
        {
            if (isAcc)
            {
                inputReceiver.Button_V_Input(1f);
            }
            else
            {
                if (isLeft)
                {
                    inputReceiver.Button_H_Input(-1f);
                }
                else
                {
                    inputReceiver.Button_H_Input(1f);
                }
            }
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (inputReceiver != null)
        {
            if (isAcc)
            {
                inputReceiver.Button_V_Input(0f);
            }
            else
            {
                inputReceiver.Button_H_Input(0f);
            }
        }
    }
}
