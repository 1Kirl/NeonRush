using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCenterOfMass : MonoBehaviour
{

    #region private val
    [SerializeField] private Rigidbody _carRB;
    #endregion





    #region private funcs

    private void OnDrawGizmos()
    {
        if (_carRB == null)
            _carRB = GetComponent<Rigidbody>();

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_carRB.worldCenterOfMass, 0.01f);
    }
    #endregion

}
