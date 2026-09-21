using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Normal,
    pickUp,
}

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTransform;

    [Header("이동 설정")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("바닥 설정")]
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private float verticalVelocity;

    private PlayerState currentState = PlayerState.Normal;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        // Camera Transform을 직접 넣지 않았으면 Main Camera를 자동으로 찾음
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }
        //상태와 관계없이 중력은 계속 적용한다.
        ApplyGravity();

        //Normal 상태가 아니면 이동 입력을 받지 않는다.
        if (currentState != PlayerState.Normal) return;

        HandleMovement(keyboard);

        // 1. WASD 입력
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


        // 2. 카메라의 앞쪽과 오른쪽 방향
        Vector3 cameraForward;
        Vector3 cameraRight;

        if (cameraTransform != null)
        {
            cameraForward = cameraTransform.forward;
            cameraRight = cameraTransform.right;
        }
        else
        {
            // 카메라를 못 찾았을 때 기본 방향 사용
            cameraForward = Vector3.forward;
            cameraRight = Vector3.right;
        }

        // 카메라의 위아래 기울기는 이동에 사용하지 않음
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();


        // 3. 카메라 기준 이동 방향
        Vector3 moveDirection =
            cameraForward * input.y +
            cameraRight * input.x;

        moveDirection =
            Vector3.ClampMagnitude(moveDirection, 1f);


        // 4. Shift 달리기
        bool isRunning = keyboard.leftShiftKey.isPressed;

        float currentSpeed =
            isRunning ? runSpeed : walkSpeed;


        // 5. 수평 이동
        controller.Move(
            moveDirection *
            currentSpeed *
            Time.deltaTime
        );


        // 6. 이동 방향으로 캐릭터 회전
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
        }


        // 7. 중력
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        controller.Move( Vector3.up * verticalVelocity * Time.deltaTime);

        // 8. Idle, Walk, Run 애니메이션
        float animationSpeed = 0f;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            animationSpeed = isRunning ? 1f : 0.5f;
        }

        animator.SetFloat("speed", animationSpeed, 0.1f, Time.deltaTime);
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

    }
    public void ChangeState(PlayerState newState)
    {
        currentState = newState;

        if (currentState != PlayerState.Normal)
        {
            animator.SetFloat("speed", 0);
        }

        Debug.Log("현재 상태 : " + currentState);
    }
}