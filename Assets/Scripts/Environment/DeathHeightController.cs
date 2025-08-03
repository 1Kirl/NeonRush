using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathHeightController : MonoBehaviour
{
    public GameObject car;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (car != null)
        {
            this.transform.position = new Vector3(
            car.transform.position.x
            , this.transform.position.y
            , car.transform.position.z);
        }
    }
}
