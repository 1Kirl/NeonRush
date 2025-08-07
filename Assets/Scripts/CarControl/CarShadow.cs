using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarShadow : MonoBehaviour
{
    private Transform car;
    [SerializeField] private Transform shadowQuad;

    [Header("그림자 설정")]
    [SerializeField] private float raySize = 15f;
    [SerializeField] private float baseFactor = 1f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2f;
    [SerializeField] private float rayOffsetY = 0.2f;     // 차 위에서 레이 발사 오프셋
    [SerializeField] private float yOffsetFromGround = 0.01f; // 바닥에서 약간 띄우기
    [SerializeField] private LayerMask groundLayerMask; // Track 레이어만 포함된 마스크
    Vector2 baseRatio;

    private Material shadowMaterial;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 1f;
    // Start is called before the first frame update
    void Start()
    {
        car = transform;
        shadowMaterial = shadowQuad.GetComponent<MeshRenderer>().material;
        baseRatio = new Vector2(shadowQuad.localScale.x,shadowQuad.localScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rayOrigin = car.position + Vector3.up * rayOffsetY;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, raySize, groundLayerMask))
        {
            Vector3 shadowPosition = hitInfo.point + Vector3.up * yOffsetFromGround;
            shadowQuad.position = shadowPosition;

            float t = Mathf.Clamp01(hitInfo.distance / raySize);
            float scaleFactor = Mathf.Lerp(minScale, maxScale, t); // 거리가 멀수록 커짐

            float alpha = Mathf.Lerp(maxAlpha, minAlpha, t);  // 가까울수록 진하고, 멀수록 옅어짐
            Color baseColor = shadowMaterial.color;
            baseColor.a = alpha;
            shadowMaterial.color = baseColor;

            shadowQuad.localScale = new Vector3(baseRatio.x * scaleFactor, 1f, baseRatio.y * scaleFactor);
            Vector3 forward = Vector3.ProjectOnPlane(car.forward, hitInfo.normal).normalized;
            shadowQuad.rotation = Quaternion.LookRotation(forward, hitInfo.normal);

            shadowQuad.gameObject.SetActive(true);
        }
        else
        {
            shadowQuad.gameObject.SetActive(false);
        }

    }
}
