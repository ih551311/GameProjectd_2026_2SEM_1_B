using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questText;
    private Quest currentQuest;

    private SimpleInventory inventory;

    private void Awake()
    {
        inventory = FindFirstObjectByType<SimpleInventory>();
    }

    private void Start()
    {
        if (questText != null && currentQuest == null)
        {
            questText.text = "진행 중인 퀘스트가 없습니다.";
        }
    }

    public void TalkToNPC(Quest npcQuest)
    {
        if (npcQuest == null) return;

        if (npcQuest.state == QuestState.Completed)
        {
            Debug.Log("이미 완료한 퀘스트입니다.");
            return;
        }

        if (currentQuest == null ||
            currentQuest.state == QuestState.Completed)
        {
            StartQuest(npcQuest);
            return;
        }

        if (currentQuest != npcQuest)
        {
            Debug.Log("이미 다른 퀘스트를 진행 중입니다.");
            return;
        }

        if (currentQuest.state == QuestState.InProgress)
        {
            Debug.Log("아직 아이템이 부족합니다.");
        }
        else if (currentQuest.state == QuestState.ReadyToComplete)
        {
            CompleteQuest();
        }
    }

    private void StartQuest(Quest newQuest)
    {
        if (newQuest == null) return;

        currentQuest = newQuest;
        currentQuest.currentAmount = inventory.GetItemAmount(currentQuest.targetItemName);

        if (currentQuest.currentAmount >= currentQuest.targetAmount)        // 미리 보유후 완료가 되었으면 완료 처리
        {
            currentQuest.currentAmount = currentQuest.targetAmount;
            currentQuest.state = QuestState.ReadyToComplete;
        }
        else
        {
            currentQuest.state = QuestState.InProgress;
        }

        Debug.Log(currentQuest.questName + " 퀘스트 시작!");
        UpdateQuestText();
    }

    public void AddQuestItem(string itemName, int amount)
    {
        if (currentQuest == null) return;
        if (currentQuest.state != QuestState.InProgress) return;
        if (currentQuest.targetItemName != itemName) return;
        if (amount <= 0) return;

        currentQuest.currentAmount += amount;

        if (currentQuest.currentAmount >= currentQuest.targetAmount)
        {
            currentQuest.currentAmount = currentQuest.targetAmount;
            currentQuest.state = QuestState.ReadyToComplete;
        }

        UpdateQuestText();
    }

    public void CompleteQuest()
    {
        if (currentQuest == null) return;
        if (currentQuest.state != QuestState.ReadyToComplete) return;

        // 퀘스트 아이템을 인벤토리에서 제출(제거)
        if (inventory.RemoveItem(currentQuest.targetItemName, currentQuest.targetAmount) == false)
        {
            Debug.Log("퀘스트 아이템이 부족합니다.");
            return;
        }

        currentQuest.state = QuestState.Completed;

        Debug.Log(currentQuest.questName + " 퀘스트 완료!");
        UpdateQuestText();
    }

    private void UpdateQuestText()
    {
        if (questText == null) return;

        if (currentQuest == null)
        {
            questText.text = "진행 중인 퀘스트가 없습니다.";
            return;
        }

        if (currentQuest.state == QuestState.InProgress)
        {
            questText.text =
                currentQuest.questName + "\n" +
                currentQuest.targetItemName + " " +
                currentQuest.currentAmount + "/" +
                currentQuest.targetAmount;
        }
        else if (currentQuest.state == QuestState.ReadyToComplete)
        {
            questText.text = "수집 완료!\nNPC에게 돌아가세요";
        }
        else if (currentQuest.state == QuestState.Completed)
        {
            questText.text =
                currentQuest.questName + "\n퀘스트 완료";
        }
        else
        {
            questText.text = "진행 중인 퀘스트가 없습니다.";
        }
    }
}