
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerPickup : MonoBehaviour
{
    [Header("직접 연결")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InteractionUI interactionUI;

    [Header("아이템 탐색")]
    [SerializeField] private float detectionRadius = 2f;

    [Header("줍기 모션")]
    [SerializeField] private float pickupDuration = 1.2f;
    [SerializeField] private float pickupMoment = 0.55f;

    private PickupItem targetItem;
    private Door targetDoor;
    private NPC targetNPC;
    private bool isPickingUp;

    // 가장 가까운 아이템 찾기
    private PickupItem FindClosestItem()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            ~0,
            QueryTriggerInteraction.Collide
        );

        PickupItem closestItem = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider col in colliders)
        {
            PickupItem item =
                col.GetComponentInParent<PickupItem>();

            if (item == null) continue;

            float distance = Vector3.Distance(
                transform.position,
                item.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestItem = item;
            }
        }

        return closestItem;
    }

    // 가장 가까운 문 찾기
    private Door FindClosestDoor()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            ~0,
            QueryTriggerInteraction.Collide
        );

        Door closestDoor = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider col in colliders)
        {
            Door door =
                col.GetComponentInParent<Door>();

            if (door == null) continue;

            float distance = Vector3.Distance(
                transform.position,
                door.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestDoor = door;
            }
        }

        return closestDoor;
    }

    // 가장 가까운 NPC 찾기
    private NPC FindClosestNPC()
    {
        Collider[] colliders = Physics.OverlapSphere(
            transform.position,
            detectionRadius,
            ~0,
            QueryTriggerInteraction.Collide
        );

        NPC closestNPC = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider col in colliders)
        {
            NPC npc =
                col.GetComponentInParent<NPC>();

            if (npc == null) continue;

            float distance = Vector3.Distance(
                transform.position,
                npc.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNPC = npc;
            }
        }

        return closestNPC;
    }

    // 상호작용 안내 문구 표시
    private void ShowGuide()
    {
        if (interactionUI == null) return;

        if (targetItem != null)
        {
            interactionUI.ShowGuide(
                "[E] " + targetItem.GetItemName() + " 줍기"
            );
        }
        else if (targetDoor != null)
        {
            interactionUI.ShowGuide(
                targetDoor.GetGuideText()
            );
        }
        else if (targetNPC != null)
        {
            interactionUI.ShowGuide(
                targetNPC.GetGuideText()
            );
        }
        else
        {
            interactionUI.HideGuide();
        }
    }

    // E키 상호작용
    private void Interact()
    {
        if (targetItem != null)
        {
            StartCoroutine(PickupAnimation());
        }
        else if (targetDoor != null)
        {
            targetDoor.Interact();
        }
        else if (targetNPC != null)
        {
            if (interactionUI != null)
            {
                interactionUI.ShowDialogue(
                    targetNPC.GetNPCName(),
                    targetNPC.GetMessage()
                );
            }
        }
    }

    // 아이템 줍기 애니메이션
    private IEnumerator PickupAnimation()
    {
        isPickingUp = true;

        // 현재 아이템 저장
        PickupItem itemToCollect = targetItem;

        if (itemToCollect == null)
        {
            isPickingUp = false;
            yield break;
        }

        // 안내 UI 숨기기
        if (interactionUI != null)
        {
            interactionUI.HideGuide();
        }

        // 아이템 방향으로 회전
        Vector3 itemDirection =
            itemToCollect.transform.position - transform.position;

        itemDirection.y = 0;

        if (itemDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation =
                Quaternion.LookRotation(itemDirection);
        }

        // 이동 금지
        if (playerController != null)
        {
            playerController.ChangeState(PlayerState.pickUp);
        }

        // 줍기 애니메이션 실행
        if (animator != null)
        {
            animator.CrossFade("PickUp01", 0.1f);
        }

        // 아이템 획득 시점 대기
        yield return new WaitForSeconds(
            Mathf.Max(0f, pickupMoment)
        );

        // 아이템 획득
        if (itemToCollect != null)
        {
            itemToCollect.Collect();
        }

        // 남은 애니메이션 시간 대기
        yield return new WaitForSeconds(
            Mathf.Max(0f, pickupDuration - pickupMoment)
        );

        // 이동 애니메이션 복귀
        if (animator != null)
        {
            animator.CrossFade("Blend Tree", 0.1f);
        }

        // 이동 허용
        if (playerController != null)
        {
            playerController.ChangeState(PlayerState.Normal);
        }

        // 상호작용 대상 초기화
        targetItem = null;
        targetDoor = null;
        targetNPC = null;

        isPickingUp = false;
    }

    // 매 프레임 실행
    void Update()
    {
        if (isPickingUp) return;

        // 주변 아이템, 문, NPC 탐색
        targetItem = FindClosestItem();
        targetDoor = FindClosestDoor();
        targetNPC = FindClosestNPC();

        // 안내 UI 표시
        ShowGuide();

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null) return;

        // 상호작용 대상이 없으면 종료
        if (targetItem == null &&
            targetDoor == null &&
            targetNPC == null)
        {
            return;
        }

        // E키를 누르면 상호작용
        if (keyboard.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }
}
