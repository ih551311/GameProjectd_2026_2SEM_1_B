using UnityEngine;

public class NPC : MonoBehaviour
{
    private string npcName = "마을 주민";

    [TextArea]

    [SerializeField] private string message = "마을에 온걸 환영해";

    public string GetNpcName()
    {
        return npcName;
    }
    public string GetMessage()
    {
        return message;
    }

    public string GetGuideText()
    {
        return "[E] " + npcName + "와 대화하기";
    }
}