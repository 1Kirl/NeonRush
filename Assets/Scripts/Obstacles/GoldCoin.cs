using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldCoin : MonoBehaviour
{
    [SerializeField] float boostL;
    [SerializeField] float boostR;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();

            var posL = other.GetComponent<Car1Controller>().SpinPointL.position;
            var posR = other.GetComponent<Car1Controller>().SpinPointR.position;

            playerRb.AddForceAtPosition(other.transform.forward * boostL, posL, ForceMode.Acceleration);
            playerRb.AddForceAtPosition(other.transform.forward * boostR, posR, ForceMode.Acceleration);

            //GameObject player = other.gameObject;
            //player.GetComponent<Rigidbody>().AddForce(transform.forward * power);

            //var playerRb = other.gameObject.GetComponent<RigidBody>();
            //playerRb.AddForce(transform.forward * power);

            UnityEngine.Debug.Log("Gold Coin Triggered " + other.name);
            Destroy(this.gameObject);
        }
    }
}
