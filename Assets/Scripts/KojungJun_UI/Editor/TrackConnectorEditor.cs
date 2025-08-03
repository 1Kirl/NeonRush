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
        if (GUILayout.Button("�ڵ����� ����Ʈ ����")) {
            AutoAssignPoints();
        }
    }

    private void AutoAssignPoints() {
        Transform tf = connector.transform;

        connector.StartPoint = tf.Find("StartPoint");
        connector.MiddlePoint = tf.Find("MiddlePoint");
        connector.EndPoint = tf.Find("EndPoint");

        if (connector.StartPoint && connector.MiddlePoint && connector.EndPoint) {
            Debug.Log($"[TrackConnectorEditor] ��� ����Ʈ �ڵ� ���� �Ϸ�: {connector.gameObject.name}");
        }
        else {
            Debug.LogWarning($"[TrackConnectorEditor] �Ϻ� ����Ʈ�� ã�� ���߽��ϴ�: {connector.gameObject.name}");
        }

        EditorUtility.SetDirty(connector);
    }

    private void OnSceneGUI() {
        if (connector == null) return;

        // �� ����Ʈ�� ������ ����
        if (connector.StartPoint && connector.MiddlePoint && connector.EndPoint) {
            // �ڵ� �̵� �����ϰ� �����
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

            // Gizmo �� �׸���
            Handles.color = Color.green;
            Handles.DrawLine(p1, p2);
            Handles.color = Color.yellow;
            Handles.DrawLine(p2, p3);
        }
    }
}
#endif
