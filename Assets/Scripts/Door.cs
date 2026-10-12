
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("문 설정")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 3f;

    private bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.localRotation;

        openRotation = closedRotation *
            Quaternion.Euler(0f, openAngle, 0f);
    }

    void Update()
    {
        Quaternion targetRotation;

        if (isOpen)
        {
            targetRotation = openRotation;
        }
        else
        {
            targetRotation = closedRotation;
        }

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            openSpeed * Time.deltaTime
        );
    }

    // 인터랙션 함수가 호출되었을 때
    public void Interact()
    {
        // bool 값을 뒤집는다.
        isOpen = !isOpen;
    }

    // 화면에 표시할 안내 문구
    public string GetGuideText()
    {
        if (isOpen)
        {
            return "[E] 문 닫기";
        }

        return "[E] 문 열기";
    }
}
