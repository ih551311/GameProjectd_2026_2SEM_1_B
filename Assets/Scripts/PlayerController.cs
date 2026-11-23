using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Normal,
    Pickup,
    Inventory,
   Attack
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTransform;

    [Header("이동 설정")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private float verticalVelocity;

    private PlayerState currentState = PlayerState.Normal;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 상태와 상관없이 중력 적용
        ApplyGravity();

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        // Normal 상태일 때만 이동 입력 처리
        if (currentState != PlayerState.Normal)
        {
            return;
        }

        HandleMovement(keyboard);
    }

    private void HandleMovement(Keyboard keyboard)
    {
        Vector2 input = Vector2.zero;

        if (keyboard.aKey.isPressed)
            input.x -= 1f;

        if (keyboard.dKey.isPressed)
            input.x += 1f;

        if (keyboard.sKey.isPressed)
            input.y -= 1f;

        if (keyboard.wKey.isPressed)
            input.y += 1f;

        input = Vector2.ClampMagnitude(input, 1f);

        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection =
            cameraForward * input.y + cameraRight * input.x;

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);

        bool isRunning = keyboard.leftShiftKey.isPressed;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        float animationSpeed = 0f;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            animationSpeed = isRunning ? 1f : 0.5f;
        }

        animator.SetFloat(
            "Speed",
            animationSpeed,
            0.1f,
            Time.deltaTime
        );
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    public void ChangeState(PlayerState newState)
    {
        currentState = newState;

        if (currentState != PlayerState.Normal)
        {
            animator.SetFloat("Speed", 0f);
        }

        Debug.Log("현재 상태 : " + currentState);
    }

    public bool IsNormalState()
    {
        return currentState == PlayerState.Normal;
    }
}