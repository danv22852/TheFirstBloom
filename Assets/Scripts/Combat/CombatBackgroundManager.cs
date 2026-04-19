using UnityEngine;

[System.Serializable]
public struct FloorBackground
{
    public string floorName;      
    public Sprite bgImage;        
}

public class CombatBackgroundManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the 2D Sprite object that acts as your combat background here.")]
    // --- CHANGED THIS LINE FROM 'Image' TO 'SpriteRenderer' ---
    public SpriteRenderer combatBackgroundSprite; 

    [Header("Background Setup")]
    public Sprite defaultBackground;
    public FloorBackground[] floorBackgrounds;

    private void Start()
    {
        SetFloorBackground();
    }

    private void SetFloorBackground()
    {
        if (combatBackgroundSprite == null) return;

        string currentFloor = "";

        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            currentFloor = GameManager.Instance.playerData.floorName;
        }

        Sprite chosenSprite = defaultBackground;

        foreach (FloorBackground fb in floorBackgrounds)
        {
            if (fb.floorName == currentFloor)
            {
                chosenSprite = fb.bgImage;
                break; 
            }
        }

        if (chosenSprite != null)
        {
            // --- UPDATED THIS LINE TO USE THE NEW VARIABLE ---
            combatBackgroundSprite.sprite = chosenSprite;
            combatBackgroundSprite.color = Color.white; 
        }
    }
}