using Shared.Bits;
using Shared.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MultiGameManager : MonoBehaviour
{
    [Header("Multiplayer")]
    [SerializeField] private GameObject _multiGameObject;
    [SerializeField] private GameObject _panel_Multi;
    [SerializeField] private GameObject _exitButton;

    private bool _enteredMatching = false;


    public void OnClick_OnlineButton() {
        if (_multiGameObject != null)
            _multiGameObject.SetActive(true);

        if (_exitButton != null)
            _exitButton.SetActive(true);

        _enteredMatching = false;
    }

    public void OnClick_StartMatching() {
        if (_panel_Multi != null)
            _panel_Multi.SetActive(true);

        _enteredMatching = true;
    }

    public void OnClick_ExitButton() {
        if (_enteredMatching) {
            // ��Ī �гθ� �ݰ� ���� �ʱ�ȭ
            if (_panel_Multi != null)
                _panel_Multi.SetActive(false);
            
            _enteredMatching = false;
        }
        else {
            // ��Ƽ ��� ��ü ����
            if (_multiGameObject != null)
                _multiGameObject.SetActive(false);

            if (_exitButton != null)
                _exitButton.SetActive(false);
        }
    }

}
