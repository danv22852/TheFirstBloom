using UnityEngine;
using System.Collections.Generic;

public enum CoreOfferSource { Combat, WorldPickup, Shop }

public class CoreOfferManager : MonoBehaviour
{
    public static CoreOfferManager Instance;

    [Header("Offer Settings")]
    public CoreTemplate[] generalCorePool;

    public static CoreOfferSource currentSource;
    public static List<CoreTemplate> pendingOffer;

    // -----------------------------
    // SINGLETON SETUP
    // -----------------------------
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Safety check for inspector setup
        if (generalCorePool == null)
        {
            Debug.LogWarning("CoreOfferManager: generalCorePool is NULL. Assign it in Inspector.");
        }
    }

    // -----------------------------
    // AUTO SPAWN IF MISSING
    // -----------------------------
    public static void EnsureExists()
    {
        if (Instance != null) return;

        var go = new GameObject("CoreOfferManager");
        go.AddComponent<CoreOfferManager>();
    }

    // -----------------------------
    // CORE OFFER GENERATION
    // -----------------------------
    public List<CoreTemplate> GenerateOffer(List<CoreTemplate> enemyPool, int count)
    {
        // 🔥 HARD GUARD: enemyPool missing
        if (enemyPool == null)
        {
            Debug.LogError("GenerateOffer FAILED: enemyPool is NULL.");
            return new List<CoreTemplate>();
        }

        // 🔥 HARD GUARD: general pool missing
        if (generalCorePool == null)
        {
            Debug.LogError("GenerateOffer FAILED: generalCorePool is NULL.");
            return new List<CoreTemplate>();
        }

        var playerOwned =
            GameManager.Instance?.playerData?.equippedCores
            ?? new List<CoreTemplate>();

        var available = new List<CoreTemplate>();

        // -----------------------------
        // ADD ENEMY POOL
        // -----------------------------
        foreach (var core in enemyPool)
        {
            if (core == null) continue;
            if (!playerOwned.Contains(core))
                available.Add(core);
        }

        // -----------------------------
        // ADD GENERAL POOL
        // -----------------------------
        foreach (var core in generalCorePool)
        {
            if (core == null) continue;
            if (!playerOwned.Contains(core) && !available.Contains(core))
                available.Add(core);
        }

        // -----------------------------
        // SHUFFLE
        // -----------------------------
        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        // -----------------------------
        // BUILD OFFER
        // -----------------------------
        var offer = new List<CoreTemplate>();

        int limit = Mathf.Min(count, available.Count);

        for (int i = 0; i < limit; i++)
        {
            if (available[i] != null)
                offer.Add(available[i]);
        }

        // Final safety log
        if (offer.Count == 0)
        {
            Debug.LogWarning("GenerateOffer returned EMPTY list.");
        }

        return offer;
    }
}