
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class SimpleInventory : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TextMeshProUGUI[] slotTexts;

    private List<InventoryItem> items = new List<InventoryItem>();

    private PlayerController playerController;
    private PlayerPickup playerPi;
    private ThirdPersonCamera thirdPersonCamera;
    private bool isOpen;


    private PlayerPickup playerPickup;
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerPickup = GetComponent<PlayerPickup>();

        if (Camera.main != null)
        {
            thirdPersonCamera = Camera.main.GetComponent<ThirdPersonCamera>();
        }
        inventoryPanel.SetActive(false);
    }

    void Start()
    {
        UpdateUi();
    }


    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null) return;

        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            Togglelnventory();
        }
    }
    private void UpdateUi()
    {
        for (int i = 0; i < slotTexts.Length; i++)
        {
            if (i < items.Count)
            {
                slotTexts[i].text = items[i].itemName + "wn" + items[i].amount + "개";
            }
            else
            {
                slotTexts[i].text = "비어 있음";
            }

        }

    }

    private void Togglelnventory()
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

            UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
        else
        {
            playerController.ChangeState(PlayerState.Normal);
            playerPickup.enabled = true;

            if (thirdPersonCamera != null)
            {
                thirdPersonCamera.enabled = true;
            }

            UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
    }

    public bool AddItem(string itemName, int amount)
    {
        foreach (InventoryItem item in items)                    //Slot을 순환하면서 아이템 이름이 같은 것이 있을 경우 수량을 + 해준다.
        {
            if (item.itemName == itemName)
            {
                item.amount += amount;
                UpdateUi();
                return true;
            }
        }

        if (items.Count >= slotTexts.Length)
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
            return false;
        }

        InventoryItem newItem = new InventoryItem();
        newItem.itemName = itemName;
        newItem.amount = amount;

        items.Add(newItem);                                      //아이템 리스트에 넣어준다.
        UpdateUi();                                              //UI를 업데이트 한다.

        return true;
    }
}
