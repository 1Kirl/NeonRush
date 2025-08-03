using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinVolumeTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Coin coin = GetComponentInParent<Coin>();
            if (coin != null)
            {
                UnityEngine.Debug.Log("Coin Triggered");
                //coin.Collect(other.gameObject);
            }
        }
    }
}
