using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private const string HorizontalAxisName = "Horizontal";
    private const string VerticalAxisName = "Vertical";
    private const float DefaultMoveSpeed = 5f;

    [SerializeField]
    [Range(1f, 15f)]
    private float moveSpeed = DefaultMoveSpeed;

    private Rigidbody playerRigidbody;
    private Vector3 movementInput;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        ReadMovementInput();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void ReadMovementInput()
    {
        float horizontalValue = Input.GetAxisRaw(HorizontalAxisName);
        float verticalValue = Input.GetAxisRaw(VerticalAxisName);

        movementInput = new Vector3(horizontalValue, 0f, verticalValue).normalized;
    }

    private void MovePlayer()
    {
        if (movementInput == Vector3.zero)
        {
            return;
        }

        Vector3 movementStep = movementInput * moveSpeed * Time.fixedDeltaTime;
        Vector3 targetPosition = playerRigidbody.position + movementStep;
        playerRigidbody.MovePosition(targetPosition);
    }
}
