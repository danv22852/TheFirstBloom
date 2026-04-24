using UnityEngine;
using UnityEngine.UI; // Needed for the Image component
using TMPro;

public class CoreUIManager : MonoBehaviour
{
    [Header("Data Source")]
    public PlayerData playerAsset; 

    [Header("UI Text")]
    public TextMeshProUGUI coreNameText;   
    public TextMeshProUGUI coreInfoText;   

    [Header("Visual Feedback")]
    public GameObject[] selectionIndicators; // The hex glows (Slot 1-5)
    
    [Header("Core Sprites")]
    // Drag the Image components that sit on your hexes here (Slot 1-5)
    public Image[] slotIcons; 

    private int currentIndex = 0;

    void OnEnable()
    {
        currentIndex = 0;
        UpdateDisplay();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            Navigate(1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            Navigate(-1);
    }

    public void Navigate(int direction)
    {
        if (playerAsset == null) return;
        currentIndex += direction;

        if (currentIndex >= playerAsset.maxCoreSlots) currentIndex = 0;
        else if (currentIndex < 0) currentIndex = playerAsset.maxCoreSlots - 1;

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (playerAsset == null || coreNameText == null || coreInfoText == null) return;

        // 1. Handle Core Icons (Turn on/off images for all 5 slots)
        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (slotIcons[i] == null) continue;

            // Check if there is actually a core in this specific list index
            if (i < playerAsset.equippedCores.Count && playerAsset.equippedCores[i] != null)
            {
                slotIcons[i].enabled = true;
                slotIcons[i].sprite = playerAsset.equippedCores[i].coreSprite;
            }
            else
            {
                // No core in this slot, hide the image
                slotIcons[i].enabled = false;
            }
        }

        // 2. Update Info Text for the currently selected slot
        bool hasCore = currentIndex < playerAsset.equippedCores.Count;
        if (hasCore && playerAsset.equippedCores[currentIndex] != null)
        {
            coreNameText.text = playerAsset.equippedCores[currentIndex].coreName;
            coreInfoText.text = playerAsset.equippedCores[currentIndex].coreDescription;
        }
        else
        {
            coreNameText.text = $"Slot {currentIndex + 1}";
            coreInfoText.text = "Empty Slot";
        }

        // 3. Update the Selection Glow
        for (int i = 0; i < selectionIndicators.Length; i++)
        {
            if (selectionIndicators[i] != null)
                selectionIndicators[i].SetActive(i == currentIndex);
        }
    }
}