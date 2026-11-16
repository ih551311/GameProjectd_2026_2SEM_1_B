using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class SimpleInventory : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private GameObject inventoryPanel;     // 인벤토리 UI 패널
    [SerializeField] private TextMeshProUGUI[] slotTexts;   // 인벤토리 슬롯에 표시할 텍스트

    [SerializeField] private TextMeshProUGUI selectedItemText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private Button useButton;
    [SerializeField] private Button dropButton;

    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private int healAmount = 30;

    private List<InventoryItem> items = new List<InventoryItem>();   // items 를 관리하는 List 자료구조 선언

    private PlayerController playerController;
    private PlayerPickup playerPickup;
    private ThirdPersonCamera thirdPersonCamera;
    private bool isOpen;

    private int selectedIndex = -1;
    // -1 : 선택 없음, 0 : 첫 번째 슬롯, 1 : 두 번째 슬롯 ~> 3 : 네 번째 슬롯

    private AchievementManager achievementManager;

    private void Awake()
    {
        achievementManager = FindFirstObjectByType<AchievementManager>();
        playerController = GetComponent<PlayerController>();
        playerPickup = GetComponent<PlayerPickup>();

        if (Camera.main != null)
        {
            thirdPersonCamera =
                Camera.main.GetComponent<ThirdPersonCamera>();
        }

        inventoryPanel.SetActive(false);
    }

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null) return;

        if (keyboard.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    private void UpdateUI()
    {
        for (int i = 0; i < slotTexts.Length; i++)
        {
            if (i < items.Count)
            {
                slotTexts[i].text =
                    items[i].itemName + "\n" + items[i].amount + "개";
            }
            else
            {
                slotTexts[i].text = "비어 있음";
            }
        }

        if (selectedIndex >= 0 && selectedIndex < items.Count)
        {
            selectedItemText.text = "선택 : " + items[selectedIndex].itemName;
            descriptionText.text = items[selectedIndex].description;

            useButton.interactable = CanUseItem(items[selectedIndex].itemName);
            dropButton.interactable = true;
        }
        else
        {
            selectedItemText.text = "아이템을 선택하세요";
            descriptionText.text = "슬롯을 클릭하면 설명이 표시 됩니다. ";

            useButton.interactable = false;
            dropButton.interactable = false;
        }
    }

    private void ToggleInventory()
    {
        if (isOpen == false && playerController.IsNormalState() == false)
        {
            return;
        }

        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        if (isOpen)
        {
            playerController.ChangeState(PlayerState.Inventory);
            playerPickup.enabled = false;

            if (thirdPersonCamera != null)
            {
                thirdPersonCamera.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            playerController.ChangeState(PlayerState.Normal);
            playerPickup.enabled = true;

            if (thirdPersonCamera != null)
            {
                thirdPersonCamera.enabled = true;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public bool AddItem(string itemName, int amount, string description = "")
    {
        // 같은 아이템이 있으면 수량 추가
        foreach (InventoryItem item in items)
        {
            if (item.itemName == itemName)
            {
                item.amount += amount;
                UpdateUI();
                return true;
            }
        }

        // 빈 슬롯이 없으면 추가 실패
        if (items.Count >= slotTexts.Length)
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
            return false;
        }

        // 새로운 아이템 생성
        InventoryItem newItem = new InventoryItem();
        newItem.itemName = itemName;
        newItem.amount = amount;
        newItem.description = description;

        items.Add(newItem);
        UpdateUI();

        return true;
    }

    // 해당 이름의 아이템을 몇 개 가지고 있는지 반환 (없으면 0)
    public int GetItemAmount(string itemName)
    {
        foreach (InventoryItem item in items)
        {
            if (item.itemName == itemName)
            {
                return item.amount;
            }
        }

        return 0;
    }

    // 슬롯 선택 함수
    public void SelectSlot(int index)
    {
        if (index < 0 || index >= items.Count) return;

        selectedIndex = index;

        UpdateUI();
    }

    public void SelectSlot0() { SelectSlot(0); }
    public void SelectSlot1() { SelectSlot(1); }
    public void SelectSlot2() { SelectSlot(2); }
    public void SelectSlot3() { SelectSlot(3); }

    private bool CanUseItem(string itemName)                    // 사용 가능한 아이템
    {
        return itemName == "물약" || itemName == "회복약" || itemName == "구급키트";
    }

    public bool RemoveItem(string itemName, int amount)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemName != itemName) continue;
            if (items[i].amount < amount) return false;

            items[i].amount -= amount;

            if (items[i].amount <= 0)
            {
                items.RemoveAt(i);
                selectedIndex = -1;
            }

            UpdateUI();
            return true;
        }

        return false;
    }

    // 아이템 사용 함수
    public void UseSelectedItem()
    {
        if (selectedIndex < 0 || selectedIndex >= items.Count) return;

        InventoryItem item = items[selectedIndex];

        if (CanUseItem(item.itemName) == false)
        {
            selectedItemText.text = item.itemName + "은(는) 사용 할 수 없습니다."; return;
        }

        playerHealth.Heal(healAmount);

        if (achievementManager != null)
        {
            achievementManager.ItemUsed(item.itemName);
        }

        RemoveItem(item.itemName, 1);
    }

    // 아이템 버리기 함수
    public void DropSelectedItem()
    {
        if (selectedIndex < 0 || selectedIndex >= items.Count) return;

        string itemName = items[selectedIndex].itemName;

        if (RemoveItem(itemName, 1))
        {
            if (achievementManager != null)
            {
                achievementManager.ItemDropped();
            }
        }
    }
}