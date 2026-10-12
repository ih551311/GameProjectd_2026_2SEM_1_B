
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerPickup : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InteractionUI interactionUI;

    [Header("상호작용 설정")]
    [SerializeField] private float detectionRadius = 2f;

    [Header("아이템 줍기 설정")]
    [SerializeField] private float pickupDuration = 1.2f;
    [SerializeField] private float pickupMoment = 0.55f;

    private PickupItem targetItem;
    private Door targetDoor;
    private NPC targetNPC;

    private bool isPickingUp;

    void Update()
    {
        if (isPickingUp) return;

        // 주변의 아이템, 문, NPC를 탐색
        targetItem = FindClosestItem();
        targetDoor = FindClosestDoor();
        targetNPC = FindClosestNPC();

        // 상호작용 안내 표시
        ShowGuide();

        // 아무것도 없으면 종료
        if (targetItem == null &&
            targetDoor == null &&
            targetNPC == null)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null) return;

        // E키 입력
        if (keyboard.eKey.wasPressedThisFrame)
        {
            Debug.Log("E키 입력 성공!");
            Interact();
        }
    }

    private void Interact()
    {
        if (targetItem != null)
        {
            StartCoroutine(PickupAnimation());
        }
        else if (targetDoor != null)
        {
            Debug.Log("문 상호작용 성공!");
            targetDoor.Interact();
        }
        else if (targetNPC != null)
        {
            Debug.Log("NPC 상호작용 성공!");

            if (interactionUI != null)
            {
                interactionUI.ShowDialogue(
                    targetNPC.GetNpcName(),
                    targetNPC.GetMessage()
                );
            }
        }
    }

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
            interactionUI.ShowGuide("");
        }
    }

    private IEnumerator PickupAnimation()
    {
        isPickingUp = true;

        PickupItem itemToCollect = targetItem;

        if (itemToCollect == null)
        {
            isPickingUp = false;
            yield break;
        }

        // 아이템 방향으로 회전
        Vector3 itemDirection =
            itemToCollect.transform.position - transform.position;

        itemDirection.y = 0f;

        if (itemDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation =
                Quaternion.LookRotation(itemDirection);
        }

        // 이동 제한
        if (playerController != null)
        {
            playerController.ChangeState(PlayerState.Pickup);
        }

        // 줍기 애니메이션
        if (animator != null)
        {
            animator.CrossFade("PickUp01", 0.1f);
        }

        yield return new WaitForSeconds(pickupMoment);

        // 아이템 획득
        if (itemToCollect != null)
        {
            itemToCollect.Collect();
        }

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

        targetItem = null;
        isPickingUp = false;
    }
}
