using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum ItemType { Coin, HealthPotion, Key }

    [Header("Item Info")]
    public ItemType type;
    public int amount = 1;

    // Unique ID so collected items don’t respawn
    [HideInInspector] public string uniqueID;

    void Start()
    {
        uniqueID = transform.position.ToString();

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
        if (collision.CompareTag("Player"))
        {
            CollectItem();
        }
    }

    private void CollectItem()
    {
        if (GameManager.Instance == null || GameManager.Instance.playerData == null) return;

        var pd = GameManager.Instance.playerData;

        // 🔑 COIN
        if (type == ItemType.Coin)
        {
            pd.coins += amount;
            Debug.Log($"Picked up {amount} coin(s)! Total Coins: {pd.coins}");
        }

        // ❤️ POTION
        else if (type == ItemType.HealthPotion)
        {
            pd.healthPotions += amount;
            Debug.Log($"Picked up {amount} potion(s)! Total Potions: {pd.healthPotions}");
        }

        // 🔑 KEY (THIS IS WHAT YOU NEEDED)
        else if (type == ItemType.Key)
        {
            pd.keys += amount;
            Debug.Log($"Picked up {amount} key(s)! Total Keys: {pd.keys}");
        }

        // Save collected item so it stays gone forever
        if (!pd.collectedItems.Contains(uniqueID))
        {
            pd.collectedItems.Add(uniqueID);
        }

        // Remove from world
        gameObject.SetActive(false);
    }
}