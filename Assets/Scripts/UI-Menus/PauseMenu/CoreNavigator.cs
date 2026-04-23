using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CoreUIManager : MonoBehaviour
{
    [Header("Data Source")]
    public PlayerData playerAsset; 

    [Header("UI Text References")]
    public TextMeshProUGUI coreNameText;   
    public TextMeshProUGUI coreInfoText;   

    [Header("Visual Feedback")]
    // Element 0 MUST be the highlight for Slot 1
    // Element 1 MUST be the highlight for Slot 2, etc.
    public GameObject[] selectionIndicators; 

    private int currentIndex = 0;

    void Start()
    {
        InitializeMenu();
    }

    void OnEnable()
    {
        InitializeMenu();
    }

    // Forces the menu to Slot 1 and refreshes text immediately
    private void InitializeMenu()
    {
        currentIndex = 0;
        UpdateDisplay();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            Navigate(1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            Navigate(-1);
        }
    }

    public void Navigate(int direction)
    {
        if (playerAsset == null) return;

        currentIndex += direction;

        // Loop logic using your maxCoreSlots variable
        if (currentIndex >= playerAsset.maxCoreSlots) currentIndex = 0;
        else if (currentIndex < 0) currentIndex = playerAsset.maxCoreSlots - 1;

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (playerAsset == null || coreNameText == null || coreInfoText == null) return;

        // 1. Update the Text based on the PlayerData List
        bool hasCoreInSlot = currentIndex < playerAsset.equippedCores.Count;

        if (hasCoreInSlot)
        {
            CoreTemplate currentCore = playerAsset.equippedCores[currentIndex];
            if (currentCore != null)
            {
                coreNameText.text = currentCore.coreName;
                coreInfoText.text = currentCore.coreDescription;
            }
        }
        else
        {
            // This overrides the "Bloom: 0 - Stable" text immediately
            coreNameText.text = $"Slot {currentIndex + 1}";
            coreInfoText.text = "Empty Slot";
        }

        // 2. Update the Highlight (The Selection Indicators)
        for (int i = 0; i < selectionIndicators.Length; i++)
        {
            if (selectionIndicators[i] != null)
            {
                // Set true ONLY if the array index matches our current slot
                selectionIndicators[i].SetActive(i == currentIndex);
            }
        }
    }
}