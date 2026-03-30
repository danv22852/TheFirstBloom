using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



// Requiring a Button component ensures we only put this on clickable skills
[RequireComponent(typeof(Button))]


public class BloomPreviewTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Skill Settings")]
    [Tooltip("The Bloom cost of THIS specific skill.")]
    public int bloomCost;

    [Header("Tooltip Settings")]
    [Tooltip("Check this ONLY for the main Attack button!")]
    public bool isBasicAttack = false;

    [TextArea(3, 5)] 
    public string actionDescription; 

    // A simple direct reference to your main system. 
    // If you use a generic GameManager instance, you can reference that instead.
    private CombatSystem combatSystem;

    private void Start()
    {
        // Find the CombatSystem in the scene automatically
        combatSystem = FindFirstObjectByType<CombatSystem>();
        
        // Safety check just in case it's missing
        if (combatSystem == null)
        {
            Debug.LogError("BloomPreviewTrigger on " + gameObject.name + " cannot find the CombatSystem in the scene!");
        }
    }

    // --- 1. THE HIGHLIGHT TRIGGERS ---
    // These functions fire when the preview should shoot out!

    // Mouse is hovering over the button
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (bloomCost > 0) combatSystem?.PreviewBloomCost(bloomCost);
        ShowMyTooltip();
    }

    // Keyboard/Controller has highlighted the button using arrow keys
    public void OnSelect(BaseEventData eventData)
    {
        if (bloomCost > 0) combatSystem?.PreviewBloomCost(bloomCost);
        ShowMyTooltip();
    }

    // --- 2. THE CLEAR TRIGGERS ---
    // These functions fire when the preview should hide back into place

    // Mouse has left the button
    public void OnPointerExit(PointerEventData eventData)
    {
        combatSystem?.ClearBloomPreview();
        combatSystem?.HideTooltip();
    }

    // Keyboard navigation moved away from this button
    public void OnDeselect(BaseEventData eventData)
    {
        combatSystem?.ClearBloomPreview();
        combatSystem?.HideTooltip();
    }

    private void ShowMyTooltip()
    {
        if (combatSystem == null) return;

        if (isBasicAttack)
        {
            // If it's the attack button, ask the CombatSystem for the exact math!
            combatSystem.ShowTooltip(combatSystem.GetBasicAttackTooltip());
        }
        else if (!string.IsNullOrEmpty(actionDescription))
        {
            // Otherwise, just show whatever you typed in the Inspector
            combatSystem.ShowTooltip(actionDescription);
        }
    }
    
}