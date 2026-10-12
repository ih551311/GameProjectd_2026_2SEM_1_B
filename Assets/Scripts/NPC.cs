
using UnityEngine;

public class NPC : MonoBehaviour
{
    [SerializeField] private string npcName = "마을 주민";

    [TextArea]
    [SerializeField] private string message = "마을에 온 것을 환영해!";

    // NPC 이름 반환
    public string GetNPCName()
    {
        return npcName;
    }

    // NPC 대사 반환
    public string GetMessage()
    {
        return message;
    }

    // NPC 안내 문구 반환
    public string GetGuideText()
    {
        return "[E] " + npcName + "와 대화하기";
    }
}
