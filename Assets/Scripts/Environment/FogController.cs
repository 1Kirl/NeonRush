using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogController : MonoBehaviour
{
    public GameObject car;
    [SerializeField] private float _zOffset;
    [SerializeField] private float _yOffset;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (car != null)
        {
            this.transform.position = new Vector3(
            car.transform.position.x
            , car.transform.position.y + _yOffset
            , car.transform.position.z + _zOffset);
        }
    }
}
