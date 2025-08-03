using UnityEngine;

public class TrackConnector : MonoBehaviour
{
    public Transform StartPoint;
    public Transform MiddlePoint;
    public Transform EndPoint;

    public TrackManager manager;
    private void Awake() {
        if (manager == null) {
            manager = FindObjectOfType<TrackManager>();
            if (manager == null) {
                Debug.LogError("TrackManager를 Hierarchy에서 찾을 수 없습니다.");
                return;
            }
        }
    }
    void Start() {
        // TrackManager 자동 할당
       
        // MiddlePoint 트리거 바인딩 (트랙 생성용)
        if (MiddlePoint != null) {
            var listener = MiddlePoint.gameObject.AddComponent<TriggerListener>();
            listener.manager = manager;
        }

    }
}
