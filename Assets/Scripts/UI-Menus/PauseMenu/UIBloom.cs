using System;
using TMPro;
using UnityEngine;

public class UIBloomDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI bloomText;

    private void Start()
    {
        // Subscribe to PlayerData instead of CombatSystem
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            GameManager.Instance.playerData.OnStatsChanged += RefreshUI;
            RefreshUI(); // Update it once immediately so it isn't blank
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks when the scene changes
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            GameManager.Instance.playerData.OnStatsChanged -= RefreshUI;
        }
    }

    private void RefreshUI()
    {
        if (GameManager.Instance == null || GameManager.Instance.playerData == null) return;

        var pd = GameManager.Instance.playerData;

        // 1. Check if the player has the alien power-up
        if (!pd.hasAlien)
        {
            bloomText.gameObject.SetActive(false); // Hide the UI completely
            return; // Stop running the rest of the visual updates
        }

        // 2. If they do have it, make sure the UI is visible
        bloomText.gameObject.SetActive(true);

        // 3. Update the text using the master PlayerData
        bloomText.text = $"Bloom: {pd.currentBloom} - {pd.currentBloomState}";

        // 4. Update the colors
        switch (pd.currentBloomState)
        {
            case BloomState.Stable:
            case BloomState.Low:
                bloomText.color = Color.white;
                break;
            case BloomState.Medium:
                bloomText.color = Color.yellow;
                break;
            case BloomState.High:
            case BloomState.Total:
                bloomText.color = Color.red;
                break;
        }
    }
}