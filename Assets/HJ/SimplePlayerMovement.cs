using UnityEngine;

public class SimpleWSADMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    
    private Rigidbody rb;
    private Vector3 movement;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        movement = new Vector3(horizontal, 0f, vertical).normalized;
    }
    
    void FixedUpdate()
    {
        if (rb != null)
        {
            // 移动
            Vector3 targetVelocity = movement * moveSpeed;
            targetVelocity.y = rb.velocity.y; // 保持垂直速度
            rb.velocity = targetVelocity;
            
            // 旋转
            if (movement.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }
}
