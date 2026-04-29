using TMPro;
using UnityEngine;

public enum StatType
{
    HP,
    Strength,
    Defense,
    Speed,
    Luck,
    wiltPotions,
    Coins,
    healthPotions,
    placeholder, // Add more stats as needed
    Level
    
}

public class UIStatText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private StatType statToDisplay;
    [SerializeField] private string prefix;

    private void OnEnable()
    {
        // Subscribe to the event when this UI element is turned on
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            GameManager.Instance.playerData.OnStatsChanged += UpdateText;
            
            // Call it once manually so the text doesn't start blank
            UpdateText(); 
        }
    }

    private void OnDisable()
    {
        // CRITICAL: Always unsubscribe when the object is disabled or destroyed.
        // If you don't do this, you will get memory leaks and null reference errors.
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            GameManager.Instance.playerData.OnStatsChanged -= UpdateText;
        }
    }

    // This method now ONLY runs when OnStatsChanged?.Invoke() is called
    private void UpdateText()
    {
        var playerData = GameManager.Instance.playerData;

        switch (statToDisplay)
        {
            case StatType.HP:
                label.text = $"{prefix}{playerData.currentHP}/{playerData.maxHP}";
                break;
            case StatType.Strength:
                label.text = $"{prefix}{playerData.strength}";
                break;
            case StatType.Defense:
                label.text = $"{prefix}{playerData.defense}";
                break;
            case StatType.Speed:
                label.text = $"{prefix}{playerData.speed}"; // Assuming you add speed to PlayerData
                break;
            case StatType.Luck:
                label.text = $"{prefix}{playerData.luck}";
                break;
            case StatType.wiltPotions:
                label.text = $"{prefix}{playerData.wiltPotions}";
                break;
            case StatType.Coins:
                label.text = $"{prefix}{playerData.coins}";
                break;
            case StatType.healthPotions:
                label.text = $"{prefix}{playerData.healthPotions}";
                break;
            case StatType.placeholder:
                label.text = $"{prefix}{playerData.thirdItem}"; // Placeholder for future stats
                break;
             case StatType.Level:
                label.text = $"{prefix}{playerData.expSystem.level}";
                break;
        }
    }
}