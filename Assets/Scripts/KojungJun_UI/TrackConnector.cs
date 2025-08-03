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
                Debug.LogError("TrackManager�� Hierarchy���� ã�� �� �����ϴ�.");
                return;
            }
        }
    }
    void Start() {
        // TrackManager �ڵ� �Ҵ�
       
        // MiddlePoint Ʈ���� ���ε� (Ʈ�� ������)
        if (MiddlePoint != null) {
            var listener = MiddlePoint.gameObject.AddComponent<TriggerListener>();
            listener.manager = manager;
        }

    }
}
