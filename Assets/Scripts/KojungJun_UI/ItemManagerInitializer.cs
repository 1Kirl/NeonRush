using System.Collections;
using UnityEngine;
using System;

public class ItemManagerInitializer : MonoBehaviour
{
    public static event Action OnAllItemManagersInitialized;

    private void Start() {
        StartCoroutine(WaitForAllManagersReady());
    }

    private IEnumerator WaitForAllManagersReady() {
        
        while (!(CarManager.Instance.IsInitialized &&
                 TrailManager.Instance.IsInitialized &&
                 DieEffectManager.Instance.IsInitialized)) {
            yield return null;
        }

        Debug.Log("[Init] All item managers loaded.");
        OnAllItemManagersInitialized?.Invoke();
    }
}
