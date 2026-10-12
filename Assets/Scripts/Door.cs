
using UnityEngine;
using TMPro;
using System.Collections;

public class InteractionUI : MonoBehaviour
{
    [Header("상호작용 안내")]
    [SerializeField] private TextMeshProUGUI guideText;

    [Header("NPC 대화")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float dialogueDuration = 3.0f;

    private Coroutine dialogueRoutine;

    void Start()
    {
        HideGuide();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    // 상호작용 안내 표시
    public void ShowGuide(string message)
    {
        if (guideText == null) return;

        guideText.gameObject.SetActive(true);
        guideText.text = message;
    }

    // 상호작용 안내 숨기기
    public void HideGuide()
    {
        if (guideText != null)
        {
            guideText.gameObject.SetActive(false);
        }
    }

    // NPC 대화창 표시
    public void ShowDialogue(string npcName, string message)
    {
        if (dialoguePanel == null || dialogueText == null)
        {
            return;
        }

        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
        }

        dialogueRoutine = StartCoroutine(
            DialogueRoutine(npcName, message)
        );
    }

    // 일정 시간 후 대화창 자동 닫기
    private IEnumerator DialogueRoutine(string npcName, string message)
    {
        dialoguePanel.SetActive(true);

        dialogueText.text = npcName + ": " + message;

        yield return new WaitForSeconds(dialogueDuration);

        dialoguePanel.SetActive(false);

        dialogueRoutine = null;
    }
}
