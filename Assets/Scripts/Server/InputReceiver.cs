using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputReceiver : MonoBehaviour
{

    #region public variable -> Packet
    public float packet_horizontalInput;
    public float packet_verticalInput;
    #endregion





    #region private variable
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
    private float _horizontalInput;
    private float _verticalInput;
    [SerializeField] private int _clientId = 99;
    #endregion



    #region event
    public event Action<float> OnHorizontalInputChanged;
    public event Action<float> OnVerticalInputChanged;

    #endregion


    #region properties
    public int ClientId
    {
        get => _clientId;
        set
        {
            Debug.Log($"[IR] InputReceiver and Client Id Binded. Id: {value}");
            _clientId = value;
            LiteNetLibManager.Instance.clientsDic[value].OnInputChanged += InputReceived;
        }
    }
    public float HorizontalInput
    {
        get => _horizontalInput;
        set
        {
            if (_horizontalInput != value)
            {
                _horizontalInput = value;
                OnHorizontalInputChanged?.Invoke(_horizontalInput);
            }

        }
    }
    public float VerticalInput
    {
        get => _verticalInput;
        set
        {
            if (_verticalInput != value)
            {
                _verticalInput = value;
                OnVerticalInputChanged?.Invoke(_verticalInput);
            }
        }
    }

    #endregion




    #region private funcs
    private void InputReceived(float h, float v)
    {
        HorizontalInput = h;
        VerticalInput = v;
    }
    private void OnSteering(InputValue value)
    {
        packet_horizontalInput = value.Get<float>();
        UnityEngine.Debug.Log("horizontalInput: " + packet_horizontalInput);
    }
    private void OnAcceleration(InputValue value)
    {
        packet_verticalInput = value.Get<float>();
        Debug.Log("verticalInput: " + packet_verticalInput);
    }

    // private void Update()
    // {
    //     VerticalInput = 
    // }
    #endregion
}
