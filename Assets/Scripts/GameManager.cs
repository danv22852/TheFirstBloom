using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] Sprite alienSprite;

    [SerializeField] private SpriteRenderer playerSpriteRenderer; // assign in Inspector

    [Header("Player Stats")]
    public int currentHP = 5;
    public int maxHP = 5;

    public int playerStrength = 15; // [cite: 3]
    public int playerSpeed = 10; // [cite: 4]
    public int playerDefense = 5; // [cite: 5]

    [Header("Bloom System")]
    public int currentBloom = 0; // [cite: 11]
    public int lowBloomThreshold = 30; // Defined arbitrary threshold for testing


    [Header("Equipment")]
    public bool hasAlien = false;
    public bool hasBow;

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
        }
    }

    private void Update()
    {
        if (hasAlien)
        {
            Debug.Log("Alien acquired!");
            playerSpriteRenderer.sprite = alienSprite;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP <= 0){currentHP = 0;
        Debug.Log("Player died" + currentHP);
        }
    }
}