using UnityEngine;
using UnityEngine.UI;

public class AlienSpriteSwapper : MonoBehaviour
{
    [Header("Data Source")]
    public PlayerData playerData; // Drag your PlayerData asset here

    [Header("Sprites")]
    public Sprite humanSprite;
    public Sprite alienSprite;

    private Image displayImage;

    void Awake()
    {
        displayImage = GetComponent<Image>();
    }

    void OnEnable()
    {
        // Subscribe to the event you created in PlayerData
        if (playerData != null)
        {
            playerData.OnStatsChanged += RefreshSprite;
        }
        
        // Initial check in case they already have the power
        RefreshSprite();
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks or errors
        if (playerData != null)
        {
            playerData.OnStatsChanged -= RefreshSprite;
        }
    }

    private void RefreshSprite()
    {
        if (playerData == null || displayImage == null) return;

        // Swap based on the bool
        displayImage.sprite = playerData.hasAlien ? alienSprite : humanSprite;
        
        Debug.Log("Sprite Refreshed. Has Alien: " + playerData.hasAlien);
    }
}