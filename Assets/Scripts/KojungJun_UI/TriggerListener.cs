using UnityEngine;

public class TriggerListener : MonoBehaviour
{
    public TrackManager manager;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other) {
        if (!triggered && other.CompareTag("Player")) {
            triggered = true;
            manager.SpawnNextTrack();
        }
    }
}
