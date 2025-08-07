using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using UnityEngine;

public class WallPusher : MonoBehaviour
{
    [Header("Pusher Config")]
    [SerializeField] private float pushDistance = 3f;      
    [SerializeField] private float pushDuration = 0.3f;    
    [SerializeField] private float intervalTime = 2f;      
    [SerializeField] private float pushForce = 15f;
    
    [Header("Movement Speed")]
    [SerializeField] private float pushOutSpeed = 20f;     
    [SerializeField] private float pullBackSpeed = 8f;     
    [SerializeField] private float holdTime = 0.2f;       
    
    [Header("Direction")]
    public Vector3 pushDirection = Vector3.right;
    
    private Vector3 hiddenPosition;   
    private Vector3 pushedPosition;  
    private Rigidbody rb;
    private bool isPushing = false;
    private bool isMoving = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        hiddenPosition = transform.position;
        pushedPosition = hiddenPosition + pushDirection.normalized * pushDistance;
        
        InvokeRepeating(nameof(PushOut), 1f, intervalTime);
    }
    
    void PushOut()
    {
        if (!isPushing && !isMoving)
        {
            StartCoroutine(PushSequence());
        }
    }
    
    private IEnumerator PushSequence()
    {
        isPushing = true;
        isMoving = true;
        
        // 1. 빠르게 튀어나오기
        yield return StartCoroutine(MoveTo(pushedPosition, pushOutSpeed));
        
        // 2. 잠깐 멈춰있기
        yield return new WaitForSeconds(holdTime);
        
        // 3. 천천히 들어가기
        yield return StartCoroutine(MoveTo(hiddenPosition, pullBackSpeed));
        
        isPushing = false;
        isMoving = false;
    }
    
    private IEnumerator MoveTo(Vector3 targetPosition, float speed)
    {
        Vector3 startPosition = transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float duration = distance / speed;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            float progress = elapsed / duration;
            
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            rb.MovePosition(currentPosition);
            
            yield return new WaitForFixedUpdate();
        }
        
        // 정확한 위치로 마지막 설정
        rb.MovePosition(targetPosition);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isPushing && other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 force = pushDirection.normalized * pushForce;
                force.y = 3f;
                
                playerRb.AddForce(force, ForceMode.Impulse);
                
                Debug.Log("플레이어 팡! 밀어냄!");
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Vector3 start = Application.isPlaying ? hiddenPosition : transform.position;
        Vector3 end = start + pushDirection.normalized * pushDistance;
        
        // 현재 상태에 따라 색상 변경
        if (isMoving)
            Gizmos.color = Color.red;
        else if (isPushing)
            Gizmos.color = Color.yellow;
        else
            Gizmos.color = Color.gray;
            
        Gizmos.DrawLine(start, end);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(end, Vector3.one * 0.8f);
        
        Gizmos.color = Color.yellow;
        Vector3 arrowEnd = end + pushDirection.normalized * 1f;
        Gizmos.DrawLine(end, arrowEnd);
    }
}
