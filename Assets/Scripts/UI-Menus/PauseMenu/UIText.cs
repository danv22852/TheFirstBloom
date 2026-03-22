using TMPro;
using UnityEngine;

public enum StatType
{
    HP,
    Strength,
    Defense,
    Speed,
    Luck
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
        }
    }
}