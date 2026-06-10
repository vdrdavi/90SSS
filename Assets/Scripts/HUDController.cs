using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    public TextMeshProUGUI weightText;
    public TextMeshProUGUI valueText;
    public GameObject inventoryPanel;
    public Transform itemContainer;
    public GameObject itemSlotPrefab;

    private Inventory playerInventory;
    private PlayerCollector playerCollector;
    private MonoBehaviour playerMovement;

    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInventory == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerInventory = player.GetComponent<Inventory>();
                playerCollector = player.GetComponent<PlayerCollector>();
                playerMovement = player.GetComponent("PlayerMovement") as MonoBehaviour;
            }
            return;
        }

        weightText.text = "Mochila: " + playerInventory.currentWeight + " / " + playerInventory.maxWeight + " kg";
        valueText.text = "Total: $" + playerInventory.totalValue;

        bool pressTab = false;
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
            pressTab = UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame;
#else
        pressTab = Input.GetKeyDown(KeyCode.Tab);
#endif

        if (pressTab)
        {
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        if (inventoryPanel == null) return;

        bool isActive = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(isActive);

        if (playerMovement != null)
        {
            playerMovement.enabled = !isActive;
        }

        Cursor.visible = isActive;
        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;

        if (isActive)
        {
            UpdateInventoryList();
        }
    }

    private void UpdateInventoryList()
    {
        if (itemContainer == null || itemSlotPrefab == null) return;

        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (ItemData item in playerInventory.collectedItems)
        {
            GameObject slot = Instantiate(itemSlotPrefab, itemContainer);

            TextMeshProUGUI slotText = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (slotText != null)
            {
                slotText.text = $"{item.itemName}\n{item.weight}kg | ${item.value}";
            }

            Button dropButton = slot.GetComponent<Button>();
            if (dropButton != null)
            {
                ItemData capturedItem = item;
                dropButton.onClick.AddListener(() => OnDropButtonClicked(capturedItem));
            }
        }
    }

    private void OnDropButtonClicked(ItemData item)
    {
        if (playerCollector != null)
        {
            if (playerCollector.TryDropItem(item))
            {
                UpdateInventoryList();
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}