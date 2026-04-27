using UnityEngine;

public class Collectible : MonoBehaviour
{
    // --- NEW: Added WiltPotion to the list! ---
    public enum ItemType { Coin, HealthPotion, WiltPotion }

    [Header("Item Info")]
    public ItemType type;
    public int amount = 1;
    
    // We use a unique ID so the game remembers exactly WHICH coin was picked up.
    [HideInInspector] public string uniqueID; 

    void Start()
    {
        // Quick Dev Trick: Use the item's exact X/Y position as its unique ID!
        // This saves you from having to manually type 100 different IDs for every coin.
        uniqueID = transform.position.ToString();

        // Check the save data. If we already picked this up, destroy it immediately!
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            if (GameManager.Instance.playerData.collectedItems.Contains(uniqueID))
            {
                gameObject.SetActive(false); 
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Make sure the Overworld Player is tagged as "Player" in the Inspector!
        if (collision.CompareTag("Player"))
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        if (GameManager.Instance == null || GameManager.Instance.playerData == null) return;

        var pd = GameManager.Instance.playerData;

        // 1. Give the item to the player
        if (type == ItemType.Coin)
        {
            pd.coins += amount;
            Debug.Log($"Picked up {amount} coin(s)! Total Coins: {pd.coins}");
        }
        else if (type == ItemType.HealthPotion)
        {
            pd.healthPotions += amount;
            Debug.Log($"Picked up {amount} potion(s)! Total Potions: {pd.healthPotions}");
        }
        // --- NEW: Give the player Wilt Potions! ---
        else if (type == ItemType.WiltPotion)
        {
            pd.wiltPotions += amount;
            
            // --- THE FIX: Tell the game the item is officially discovered! ---
            pd.hasDiscoveredWiltPotions = true; 
            
            Debug.Log($"Picked up {amount} Wilt Potion(s)! Total Wilt Potions: {pd.wiltPotions}");
        }

        // 2. Save this item's ID to the graveyard so it never respawns
        if (!pd.collectedItems.Contains(uniqueID))
        {
            pd.collectedItems.Add(uniqueID);
        }

        // Optional: If you have an audio source or particle effect, trigger it here!

        // 3. Poof! Vanish from the world.
        gameObject.SetActive(false);
    }
}