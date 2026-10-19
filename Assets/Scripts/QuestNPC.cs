using UnityEngine;
using UnityEngine.InputSystem;

public class QuestNPC : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Quest quest;
    [SerializeField] private Transform player;
    [SerializeField] private float talkDistance = 3f;

    private bool playerInNear;

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        bool isNear = distance <= talkDistance;

        if (isNear && !playerInNear)
        {
            Debug.Log("E키를 눌러 대화하세요");
        }

        playerInNear = isNear;

        if (!playerInNear) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.eKey.wasPressedThisFrame)
        {
            if (questManager == null)
            {
                Debug.LogWarning("Quest Manager를 연결해주세요.");
                return;
            }

            questManager.TalkToNPC(quest);
        }
    }
}