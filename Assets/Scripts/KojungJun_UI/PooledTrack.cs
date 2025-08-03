using UnityEngine;

public class PooledTrack : MonoBehaviour
{
    public void ReturnToPool() {
        gameObject.SetActive(false);
    }
}
