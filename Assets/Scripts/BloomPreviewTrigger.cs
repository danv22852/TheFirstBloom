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

    private CombatSystem combatSystem;
    
    // --- NEW: A reference to the Core this button is holding! ---
    private CoreTemplate assignedCore; 

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (bloomCost > 0) combatSystem?.PreviewBloomCost(bloomCost);
        ShowMyTooltip();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (bloomCost > 0) combatSystem?.PreviewBloomCost(bloomCost);
        ShowMyTooltip();
    }

    // --- 2. THE CLEAR TRIGGERS ---

    public void OnPointerExit(PointerEventData eventData)
    {
        combatSystem?.ClearBloomPreview();
        combatSystem?.HideTooltip();
    }

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
        else if (assignedCore != null)
        {
            // --- NEW: If this button has a Core, ask the Core for its dynamic math! ---
            combatSystem.ShowTooltip(assignedCore.GetDynamicDescription(combatSystem));
        }
        else if (!string.IsNullOrEmpty(actionDescription))
        {
            // Otherwise, just show whatever you typed in the Inspector (used for Items/Run)
            combatSystem.ShowTooltip(actionDescription);
        }
    }
    
    public void Setup(CoreTemplate core, CombatSystem system)
    {
        combatSystem = system;
        assignedCore = core; // --- NEW: Save the Core reference! ---
        bloomCost = core.bloomCost;
        
        // Clear the static text so the dynamic text takes over
        actionDescription = ""; 

        var label = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (label != null) label.text = core.coreName;

        var button = GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => system.UseCore(core));
    }

    public void SetupEmpty()
    {
        assignedCore = null; // --- NEW: Clear the core reference just in case! ---

        var label = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (label != null) label.text = "Empty";

        var button = GetComponent<UnityEngine.UI.Button>();
        
        // Keep button active for navigation but block clicking
        button.onClick.RemoveAllListeners();
        
        // Grey out visually
        var colors = button.colors;
        colors.normalColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        colors.selectedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        button.colors = colors;

        // Block tooltip and bloom preview
        bloomCost = 0;
        actionDescription = "";
    }
}