using UnityEngine;



public class coreTutorial : MonoBehaviour

{
    
    [Header("Cutscene Dialogue")]
    public DialogueLine[] encounterDialogue;
    void Update()
    { 
        if (GameManager.Instance.playerData.finishedTutorial && GameManager.Instance.playerData.hasAlien)
        {
            PlayCutscene();
            Destroy(gameObject); // No need for this cutscene if the tutorial is already done
        }
    }

    private void PlayCutscene()
    {
        // --- NEW: Lock player movement when the cutscene starts ---
        

        if (encounterDialogue != null && encounterDialogue.Length > 0)
        {
            // Start dialogue and pass StartChasing to run when it finishes
            DialogueManager.instance.StartDialogue(encounterDialogue);
        }
        
    }

    
}
