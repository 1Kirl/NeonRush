using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class WavyText : MonoBehaviour
{
    public float amplitude = 5.0f;
    public float frequency = 2.0f;
    public float speed = 2.0f;

    private TextMeshProUGUI textMesh;
    private TMP_TextInfo textInfo;

    void Awake() {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    void Update() {
        textMesh.ForceMeshUpdate();
        textInfo = textMesh.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++) {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int materialIndex = charInfo.materialReferenceIndex;

            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            for (int j = 0; j < 4; j++) {
                Vector3 offset = new Vector3(0, Mathf.Sin(Time.time * speed + vertices[vertexIndex + j].x * frequency) * amplitude, 0);
                vertices[vertexIndex + j] += offset;
            }
        }

        // Àû¿ë
        for (int i = 0; i < textInfo.meshInfo.Length; i++) {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
}
