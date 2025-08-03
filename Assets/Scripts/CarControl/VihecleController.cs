using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class VehicleController : MonoBehaviour
{
    private InputPacket _input;
    [SerializeField]
    private float _currentSpeed = 0f;
    [SerializeField]
    public float gasPower = 0.1f; 
    [SerializeField]
    private UnityEngine.Vector3 _constrainedAxis;
    [SerializeField] private float _maxSpeed = 10f;
    private CharacterController _controller;
    private Vector3 fixedRotation = new Vector3(0, 0, 0);
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<InputPacket>();
    }

    void Update()
    {
        transform.rotation = Quaternion.Euler(fixedRotation);
        MoveForward();
    }
    private void MoveForward()
    {
        if (_input.gas && _currentSpeed < _maxSpeed)
        {
            Debug.Log("gas pressed");
            //rb.AddForce(transform.forward * gasPower, ForceMode.Acceleration);
            UnityEngine.Vector3 moveDirection = transform.forward * 5f * Time.deltaTime;
            _controller.Move(moveDirection);
        }
    }

}
