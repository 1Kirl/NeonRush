#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrackConnector))]
public class TrackConnectorEditor : Editor
{
    private TrackConnector connector;

    private void OnEnable() {
        connector = (TrackConnector)target;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GUILayout.Space(10);
        if (GUILayout.Button("자동으로 포인트 연결")) {
            AutoAssignPoints();
        }
    }

    private void AutoAssignPoints() {
        Transform tf = connector.transform;

        connector.StartPoint = tf.Find("StartPoint");
        connector.MiddlePoint = tf.Find("MiddlePoint");
        connector.EndPoint = tf.Find("EndPoint");

        if (connector.StartPoint && connector.MiddlePoint && connector.EndPoint) {
            Debug.Log($"[TrackConnectorEditor] 모든 포인트 자동 연결 완료: {connector.gameObject.name}");
        }
        else {
            Debug.LogWarning($"[TrackConnectorEditor] 일부 포인트를 찾지 못했습니다: {connector.gameObject.name}");
        }

        EditorUtility.SetDirty(connector);
    }

    private void OnSceneGUI() {
        if (connector == null) return;

        // 각 포인트가 존재할 때만
        if (connector.StartPoint && connector.MiddlePoint && connector.EndPoint) {
            // 핸들 이동 가능하게 만들기
            EditorGUI.BeginChangeCheck();

            Vector3 p1 = Handles.PositionHandle(connector.StartPoint.position, Quaternion.identity);
            Vector3 p2 = Handles.PositionHandle(connector.MiddlePoint.position, Quaternion.identity);
            Vector3 p3 = Handles.PositionHandle(connector.EndPoint.position, Quaternion.identity);

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(connector.StartPoint, "Move StartPoint");
                Undo.RecordObject(connector.MiddlePoint, "Move MiddlePoint");
                Undo.RecordObject(connector.EndPoint, "Move EndPoint");

                connector.StartPoint.position = p1;
                connector.MiddlePoint.position = p2;
                connector.EndPoint.position = p3;
            }

            // Gizmo 선 그리기
            Handles.color = Color.green;
            Handles.DrawLine(p1, p2);
            Handles.color = Color.yellow;
            Handles.DrawLine(p2, p3);
        }
    }
}
#endif
