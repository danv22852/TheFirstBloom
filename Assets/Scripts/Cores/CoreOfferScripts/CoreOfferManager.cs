using UnityEngine;
using System.Collections.Generic;

public enum CoreOfferSource { Combat, WorldPickup, Shop }

public class CoreOfferManager : MonoBehaviour
{
    public static CoreOfferManager Instance;
    
    [Header("Offer Settings")]
    public CoreTemplate[] generalCorePool;
    
    // Set these before loading the CorePopup scene
    public static CoreOfferSource currentSource;
    public static List<CoreTemplate> pendingOffer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<CoreTemplate> GenerateOffer(List<CoreTemplate> enemyPool, int count)
    {
        var playerOwned = GameManager.Instance?.playerData?.equippedCores ?? new List<CoreTemplate>();

        // Combine enemy pool and general pool, filter out already owned
        var available = new List<CoreTemplate>();

        foreach (var core in enemyPool)
            if (core != null && !playerOwned.Contains(core))
                available.Add(core);

        foreach (var core in generalCorePool)
            if (core != null && !playerOwned.Contains(core) && !available.Contains(core))
                available.Add(core);

        // Shuffle
        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = available[i];
            available[i] = available[j];
            available[j] = temp;
        }

        // Return up to count cores
        var offer = new List<CoreTemplate>();
        for (int i = 0; i < Mathf.Min(count, available.Count); i++)
            offer.Add(available[i]);

        return offer;
    }
}