using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum VendingItemType { Core, HealthPotion, WiltPotion }

[System.Serializable]
public class VendingSlot 
{
    public string slotCode;      
    public int price;            
    public VendingItemType itemType; 
    
    [Header("If Core Type")]
    public CoreTemplate core;    

    [Header("Visuals")]
    public Image displayImage;   // This is your "White Cube" image
    public Sprite itemSprite;    // The actual icon (potion, etc.) to show on the cube
    
    public bool inStock = true;
}

public class VendingMachine : MonoBehaviour
{
    [Header("Data Reference")]
    public PlayerData playerData; 

    [Header("UI Displays")]
    public TextMeshProUGUI topDisplay;    
    public TextMeshProUGUI bottomDisplay; 

    [Header("Vending Setup")]
    public List<VendingSlot> slots;
    private Dictionary<string, VendingSlot> slotDictionary = new Dictionary<string, VendingSlot>();

    private string currentInput = "";
    private bool isDispensing = false;

    void Awake()
    {
        slotDictionary.Clear();
        foreach (var slot in slots)
        {
            if (!string.IsNullOrEmpty(slot.slotCode))
            {
                if (!slotDictionary.ContainsKey(slot.slotCode))
                    slotDictionary.Add(slot.slotCode, slot);
                
                if (slot.displayImage != null)
                {
                    if (!slot.inStock)
                    {
                        slot.displayImage.gameObject.SetActive(false);
                    }
                    else
                    {
                        slot.displayImage.gameObject.SetActive(true);
                        slot.displayImage.sprite = slot.itemSprite;
                    }
                }
            }
        }
    }

    void Start() => ResetDisplay();

    void Update()
    {
        // --- NEW: Keyboard Shortcuts for Closing ---
        // We only check this if the UI is actually active
        if (gameObject.activeSelf && !isDispensing)
        {
            if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.P))
            {
                OnBackButtonPressed();
            }
        }
    }

    public void OnKeyPressed(string input)
    {
        if (isDispensing) return;
        if (currentInput.Length < 2)
        {
            currentInput += input;
            topDisplay.text = currentInput;
        }
        if (currentInput.Length == 2) ValidateSelection();
    }

    private void ValidateSelection()
    {
        if (slotDictionary.TryGetValue(currentInput, out VendingSlot slot))
        {
            bottomDisplay.text = slot.inStock ? "PRICE: " + slot.price : "SOLD OUT";
        }
        else
        {
            bottomDisplay.text = "INVALID";
        }
    }

    public void OnClearPressed() 
    { 
        if (!isDispensing) ResetDisplay(); 
    }

    public void OnConfirmPressed()
    {
        if (isDispensing || currentInput.Length < 2 || playerData == null) return;

        if (slotDictionary.TryGetValue(currentInput, out VendingSlot slot))
        {
            if (slot.inStock && playerData.coins >= slot.price)
                StartCoroutine(DispenseRoutine(slot));
            else if (!slot.inStock)
                bottomDisplay.text = "OUT OF STOCK";
            else
                bottomDisplay.text = "NOT ENOUGH COINS";
        }
    }

    public void OnBackButtonPressed()
    {
        // Find the trigger in the scene to resume time and hide UI
        VendingTrigger trigger = Object.FindFirstObjectByType<VendingTrigger>();
        if (trigger != null)
        {
            trigger.CloseVendingUI();
        }
        else
        {
            // Fallback if no trigger script is found
            gameObject.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    private IEnumerator DispenseRoutine(VendingSlot slot)
    {
        isDispensing = true;
        playerData.coins -= slot.price;
        topDisplay.text = "PURCHASING...";
        
        string itemName = (slot.itemType == VendingItemType.Core) ? "CORE PACK" : (slot.itemType == VendingItemType.HealthPotion) ? "HEALTH POTION" : "WILT POTION";
        bottomDisplay.text = itemName;

        yield return new WaitForSecondsRealtime(1.2f); // Use Realtime because Time.timeScale is 0!

        if (slot.displayImage != null)
            slot.displayImage.gameObject.SetActive(false);

        slot.inStock = false;
        
        if (slot.itemType == VendingItemType.Core)
        {
            CoreOfferManager.currentSource = CoreOfferSource.Shop;
            CoreOfferManager.pendingOffer = CoreOfferManager.Instance.GenerateOffer(new List<CoreTemplate>(), 3);
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("CorePopup", LoadSceneMode.Additive);
        }
        else if (slot.itemType == VendingItemType.HealthPotion)
        {
            playerData.healthPotions++;
        }
        else if (slot.itemType == VendingItemType.WiltPotion)
        {
            playerData.wiltPotions++;
        }

        yield return new WaitForSecondsRealtime(1.0f);
        ResetDisplay();
        isDispensing = false;
    }

    private void ResetDisplay()
    {
        currentInput = "";
        topDisplay.text = "Make your selection";
        bottomDisplay.text = "";
    }
}