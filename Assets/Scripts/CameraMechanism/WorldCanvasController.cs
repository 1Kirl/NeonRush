using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldCanvasController : MonoBehaviour
{
    [SerializeField] private float _planeDistance = 3f;
    [SerializeField] private GameObject _particle;
    RectTransform rect;

    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
        ResizeCanvasToFitCamera();
    }
    public void ShowEffect()
    {
        Instantiate(_particle, rect);

    }
    private void ResizeCanvasToFitCamera()
    {
        Camera targetCamera = Camera.main;
        float height = 2f * _planeDistance * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * targetCamera.aspect;

        rect.sizeDelta = new Vector2(width, height);
        rect.localPosition = new Vector3(0f, 0f, _planeDistance);
        rect.localRotation = Quaternion.identity;
    }
}
