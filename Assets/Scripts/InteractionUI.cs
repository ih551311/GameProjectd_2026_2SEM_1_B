using UnityEngine;
using TMPro;
using System.Collections;

public class InteractionUI : MonoBehaviour
{
    [Header("UI 관련 기본 정보")]
    [Tooltip("괜히 건드리진 말자")]

    [SerializeField] private TextMeshProUGUI guideText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float dialogueDuration = 3.0f;     // 3초동안 보여주고 사라질 것 (보여주고 지울 예정)

    private Coroutine dialogueRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HideGuide();
        dialoguePanel.SetActive(false);
    }

    public void ShowGuide(string message)
    {
        guideText.gameObject.SetActive(true);
        guideText.text = message;
    }

    public void HideGuide()
    {
        guideText.gameObject.SetActive(false);
    }

    // Update 는 코루틴만을 사용하기에 없앤다.

    public void ShowDialogue(string npcName, string message)
    {
        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);

        }

        dialogueRoutine = StartCoroutine(DialogueRoutine(npcName, message));
    }

    private IEnumerator DialogueRoutine(string npcName, string message)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = npcName + "\n" + message;

        yield return new WaitForSeconds(dialogueDuration);

        dialoguePanel.SetActive(false);
        dialogueRoutine = null;
    }

}