
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

        foreach (Collider itemCollider in colliders)
        {
            PickupItem item =
                itemCollider.GetComponentInParent<PickupItem>();

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

        foreach (Collider targetCollider in colliders)
        {
            Door door =
                targetCollider.GetComponentInParent<Door>();

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

        foreach (Collider targetCollider in colliders)
        {
            NPC npc =
                targetCollider.GetComponentInParent<NPC>();

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

    // 아이템 줍기 애니메이션
    private IEnumerator PickupAnimation()
    {
        isPickingUp = true;

        // 현재 찾은 아이템 저장
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

        // 이동 제한
        if (playerController != null)
        {
            playerController.ChangeState(PlayerState.pickUp);
        }

        // 줍기 애니메이션 실행
        if (animator != null)
        {
            animator.CrossFade("PickUp01", 0.1f);
        }

        // 손이 아이템에 닿는 순간까지 기다림
        yield return new WaitForSeconds(
            Mathf.Max(0f, pickupMoment)
        );

        // 아이템 획득
        if (itemToCollect != null)
        {
            itemToCollect.Collect();
        }

        // 남은 애니메이션 시간
        yield return new WaitForSeconds(
            Mathf.Max(0f, pickupDuration - pickupMoment)
        );

        // 이동 애니메이션 복귀
        if (animator != null)
        {
            animator.CrossFade("Blend Tree", 0.1f);
        }

        // 플레이어 이동 허용
        if (playerController != null)
        {
            playerController.ChangeState(PlayerState.Normal);
        }

        targetItem = null;
        isPickingUp = false;
    }

    void Update()
    {
        if (isPickingUp) return;

        // 가장 가까운 아이템 탐색
        targetItem = FindClosestItem();

        // 가장 가까운 문 탐색
        targetDoor = FindClosestDoor();

        // 가장 가까운 NPC 탐색
        targetNPC = FindClosestNPC();

        // 안내 UI 표시
        if (interactionUI != null)
        {
            ShowGuide();
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null) return;

        // E키로 아이템 줍기
        if (targetItem != null &&
            keyboard.eKey.wasPressedThisFrame)
        {
            StartCoroutine(PickupAnimation());
        }
    }
}
