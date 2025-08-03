using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{


    #region Definitions

    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    #endregion



    #region Public Variables

    #endregion



    #region Serialized Variables

    #endregion



    #region Private Variables

    #endregion



    #region Properties

    #endregion



    #region Mono Behaviours

    // Prevent this GameObject from being destroyed between scene changes
    private void Awake() {
        DontDestroyOnLoad(this.gameObject);
    }


    // Process all queued actions every frame
    private void Update() {
        while (_executionQueue.Count > 0) {
            Action action;
            lock (_executionQueue) {
                action = _executionQueue.Dequeue();
            }
            action?.Invoke();
        }
    }

    #endregion



    #region Public Functions

    // Enqueue an action to be executed on the main thread
    public static void Enqueue(Action action) {
        lock (_executionQueue) {
            _executionQueue.Enqueue(action);
        }
    }

    #endregion



    #region Private Functions

    #endregion
}
