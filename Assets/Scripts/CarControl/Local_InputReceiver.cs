using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class Local_InputReceiver : MonoBehaviour
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
    private bool _driftKeyInput = false;
    #endregion



    #region event
    public event Action<float> OnHorizontalInputChanged;
    public event Action<float> OnVerticalInputChanged;
    public event Action<bool> OnDriftInputChanged;

    #endregion


    #region properties

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
    public bool DriftKeyInput
    {
        get => _driftKeyInput;
        set
        {
            if (_driftKeyInput != value)
            {
                _driftKeyInput = value;
                OnDriftInputChanged?.Invoke(_driftKeyInput);
            }
        }
    }
    #endregion



    public void Button_H_Input(float value)
    {
        packet_horizontalInput = value;
        HorizontalInput = value;
        UnityEngine.Debug.Log("Button horizontalInput: " + HorizontalInput);
    }
    public void Button_V_Input(float value)
    {
        packet_verticalInput = value;
        VerticalInput = value;
        Debug.Log("Button verticalInput: " + VerticalInput);
    }

    #region private funcs
    private void OnSteering(InputValue value)
    {
        packet_horizontalInput = value.Get<float>();
        HorizontalInput = value.Get<float>();
        UnityEngine.Debug.Log("horizontalInput: " + HorizontalInput);
    }
    private void OnAcceleration(InputValue value)
    {
        packet_verticalInput = value.Get<float>();
        VerticalInput = value.Get<float>();
        Debug.Log("verticalInput: " + VerticalInput);
    }
    private void OnDriftKey(InputValue value)
    {
        //packet_horizontalInput = value.Get<bool>();
        float raw = value.Get<float>();
        DriftKeyInput = raw > 0.5f;
        UnityEngine.Debug.Log("driftkey: " + DriftKeyInput);
    }
    #endregion

}
