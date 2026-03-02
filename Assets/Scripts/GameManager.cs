using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Stats")]
    public int currentHP = 5;
    public int maxHP = 5;

    [Header("Equipment")]
    public bool hasAlien;
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

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        if (currentHP <= 0){currentHP = 0;
        Debug.Log("Player died" + currentHP);
        }
    }
}