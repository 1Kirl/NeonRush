using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MudZone : MonoBehaviour
{
    [Header("Heavy Car Settings (Mass ~1500)")]
    [SerializeField] private float velocityReduction = 0.95f;  // 속도를 5%씩 감소
    [SerializeField] private float minSpeed = 2f;              // 최소 속도 (완전히 멈추지 않게)
    [SerializeField] private float strongDragForce = 12000f;   // 강한 드래그
    
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Wheel"))
        {
            Rigidbody carRb = FindRigidbody(other.transform);
            if (carRb != null)
            {
                ApplyMudEffect(carRb);
            }
        }
    }
    
    private void ApplyMudEffect(Rigidbody carRb)
    {
        Vector3 velocity = carRb.velocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        
        // 방법 1: 속도 직접 감소 (더 확실한 효과)
        if (horizontalVelocity.magnitude > minSpeed)
        {
            Vector3 newVelocity = horizontalVelocity * velocityReduction;
            newVelocity.y = velocity.y; // Y축은 그대로 유지
            carRb.velocity = newVelocity;
        }
        
        // 방법 2: 추가 드래그 (보조 효과)
        if (horizontalVelocity.magnitude > 0.1f)
        {
            Vector3 dragForce = -horizontalVelocity.normalized * strongDragForce * Time.fixedDeltaTime;
            carRb.AddForce(dragForce, ForceMode.Force);
        }
        
        // 디버그 로그 (필요시)
        // Debug.Log($"진흙 효과 - 속도: {carRb.velocity.magnitude:F1}");
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wheel"))
        {
            BoxCollider box = GetComponent<BoxCollider>();
            Vector3 realSize = Vector3.Scale(box.size, transform.lossyScale);
            Debug.Log($"[HeavyCarMudZone] 진입! 오브젝트: {gameObject.name}");
            Debug.Log($"Transform Scale: {transform.lossyScale}");
            Debug.Log($"BoxCollider Size: {box.size}");
            Debug.Log($"실제 충돌 크기: {realSize}");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wheel"))
        {
            Debug.Log("무거운 차량이 진흙에서 탈출!");
        }
    }
    
    private Rigidbody FindRigidbody(Transform findingRb)
    {
        Rigidbody rb = null;
        while (findingRb != null)
        {
            rb = findingRb.GetComponent<Rigidbody>();
            if (rb != null) break;
            findingRb = findingRb.parent;
        }
        return rb;
    }
    
    void OnDrawGizmos() // Selected 제거해서 항상 보이게
    {
        Gizmos.color = new Color(0.6f, 0.4f, 0.2f, 0.3f);
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            // 실제 충돌 크기 계산 (Scale 반영)
            Vector3 realSize = Vector3.Scale(box.size, transform.lossyScale);
            Gizmos.DrawCube(transform.position + box.center, realSize);
            
            // 테두리도 표시
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + box.center, realSize);
        }
    }
}