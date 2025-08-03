using UnityEngine;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour
{
    public Transform initialSpawnPoint;

    [System.Serializable]
    public class TrackEntry
    {
        public string trackName;
    }

    public List<TrackEntry> availableTracks;

    private Vector3 nextSpawnPosition;
    private string lastTrackName = "";
    private List<(string name, GameObject instance)> activeTracks = new();
    private Transform lastEndPoint;
    private GameObject currentTrack;
    private string currentTrackName;

    private GameObject previousTrack;
    private string previousTrackName;

    private GameObject twoTracksAgo;
    private string twoTracksAgoName;


    void Start() {
        nextSpawnPosition = initialSpawnPoint.position;
    }

    public void BeginTrackSpawning() {
        nextSpawnPosition = initialSpawnPoint.position;
        lastEndPoint = initialSpawnPoint; 
        activeTracks.Clear();
        SpawnNextTrack();
    }


    public void SpawnNextTrack() {
        var candidates = availableTracks.FindAll(t => t.trackName != lastTrackName);
        if (candidates.Count == 0) {
            Debug.LogWarning("�ĺ� Ʈ�� ����, �ߺ� ���");
            candidates = availableTracks;
        }

        var next = candidates[Random.Range(0, candidates.Count)];
        lastTrackName = next.trackName;

        // Instantiate �ӽ÷� (ȸ���� �ϴ� 0)
        GameObject newTrack = TrackPoolManager.Instance.GetFromPool(next.trackName, Vector3.zero, Quaternion.identity);

        // ����Ʈ ����
        TrackConnector connector = newTrack.GetComponent<TrackConnector>();
        Transform newStart = connector.StartPoint;

        // ȸ�� ���� ���� (���� ����)
        Quaternion targetRotation = Quaternion.LookRotation(lastEndPoint.forward, Vector3.up);
        Quaternion newStartRotation = Quaternion.LookRotation(newStart.forward, Vector3.up);
        Quaternion rotationDelta = targetRotation * Quaternion.Inverse(newStartRotation);
        newTrack.transform.rotation = rotationDelta * newTrack.transform.rotation;

        // ��ġ ���� (EndPoint�� StartPoint ���߱�)
        Vector3 offset = newStart.position - newTrack.transform.position;
        newTrack.transform.position = lastEndPoint.position - offset;
        if (previousTrack != null) {
            TrackPoolManager.Instance.ReturnToPool(previousTrackName, previousTrack);
        }
        // ���� ����
        if (twoTracksAgo != null) {
            TrackPoolManager.Instance.ReturnToPool(twoTracksAgoName, twoTracksAgo);
        }

        twoTracksAgo = previousTrack;
        twoTracksAgoName = previousTrackName;

        previousTrack = currentTrack;
        previousTrackName = currentTrackName;

        currentTrack = newTrack;
        currentTrackName = next.trackName;

        activeTracks.Add((next.trackName, newTrack)); // �ʿ信 ���� ����
        lastEndPoint = connector.EndPoint;
    }

    public void ClearTracksOnGameOver() {
        foreach (var (_, trackObj) in activeTracks) {
            if (trackObj != null) Destroy(trackObj);
        }

        activeTracks.Clear();
    }

}
