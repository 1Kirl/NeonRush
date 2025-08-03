using System;
using Shared.Protocol;
using UnityEngine;
namespace Shared.Network
{
    public class clientData
    {
        public float horizontalInput = 0f, verticalInput = 0f;
        public Vector3 pos;
        public Quaternion rot;
        public int clientId = 99; 
        public int carKind = 0;
        public int DieEffect = 0;
        public int Trail = 0;
        public string name = "";
        public bool isMe = false;
        public event Action<float, float> OnInputChanged;
        public event Action<Vector3, Quaternion> OnTransformUpdate;
        public event Action<EffectType> OnEffectActive;
        public void InvokeInputEvent()
        {
            OnInputChanged?.Invoke(horizontalInput, verticalInput);
        }
        public void InvokeTransformEvent()
        {
            OnTransformUpdate?.Invoke(pos, rot);
        }
        public void InvokeEffectEvent(EffectType effectType)
        {
            OnEffectActive?.Invoke(effectType);
        }
    }
}
