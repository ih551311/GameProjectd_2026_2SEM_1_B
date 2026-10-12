
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Normal,
    pickUp
}

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 5f;
    public float jumpPower = 5f;
    public float gravity = -20f;

    [Header("카메라")]
    public float mouseSensitivity = 0.2f;
    public Transform cameraPivot;
    public Transform cameraTransform;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isRunning;
    private float pitch = 20f;
    private float verticalVelocity;
    private CharacterController controller;
    private PlayerState currentState = PlayerState.Normal;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (currentState != PlayerState.Normal) return;

        if (value.isPressed && controller != null && controller.isGrounded)
        {
            verticalVelocity = jumpPower;
        }
    }

    public void OnSprint(InputValue value)
    {
        isRunning = value.isPressed;
    }

    // 플레이어 상태 변경
    public void ChangeState(PlayerState newState)
    {
        currentState = newState;

        if (currentState != PlayerState.Normal)
        {
            moveInput = Vector2.zero;
        }
    }

    void Update()
    {
        if (controller == null) return;

        // 바닥에 닿았을 때 중력 처리
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        if (currentState == PlayerState.Normal)
        {
            // 마우스로 시점 회전
            transform.Rotate(
                Vector3.up,
                lookInput.x * mouseSensitivity
            );

            pitch = Mathf.Clamp(
                pitch - lookInput.y * mouseSensitivity,
                -20f,
                60f
            );

            if (cameraPivot != null)
            {
                cameraPivot.localRotation =
                    Quaternion.Euler(pitch, 0f, 0f);
            }

            // 카메라 기준 이동
            Transform view =
                cameraTransform != null ? cameraTransform : transform;

            Vector3 forward = view.forward;
            Vector3 right = view.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 movement =
                forward * moveInput.y +
                right * moveInput.x;

            movement = Vector3.ClampMagnitude(movement, 1f);

            // 달리기 속도 적용
            float speed = isRunning ? moveSpeed * 2f : moveSpeed;

            controller.Move(
                (movement * speed +
                Vector3.up * verticalVelocity) *
                Time.deltaTime
            );
        }
        else
        {
            // 아이템 줍는 동안 이동 제한
            controller.Move(
                Vector3.up * verticalVelocity * Time.deltaTime
            );
        }
    }
}
