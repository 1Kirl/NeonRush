using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputPacket : MonoBehaviour
{
    public bool gas;
    
#if ENABLE_INPUT_SYSTEM
    public void OnGas(InputValue value)
    {
        Gas(value.isPressed);
    }
#endif
    public void Gas(bool gassPressed)
    {
        gas = gassPressed;
    } 
}
