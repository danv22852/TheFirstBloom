using System; // REQUIRED FOR CALLBACKS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct DialogueLine
{
    [TextArea(3, 10)]
    public string text;
    public Sprite portrait;
    public string name;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance; 

    [Header("UI References")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image portraitImage;
    
    [Header("Settings")]
    public float typingSpeed = 0.04f;

    private Queue<DialogueLine> linesQueue;
    private bool isTyping = false;
    private DialogueLine currentLine;
    
    // --- NEW: A variable to store what should happen after dialogue ends ---
    private Action onDialogueComplete;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        
        linesQueue = new Queue<DialogueLine>();
    }

    void Update()
    {
        nameText.text = currentLine.name;

        if (dialogueBox.activeInHierarchy && (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.O)))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = currentLine.text;
                isTyping = false;
            }
            else
            {
                DisplayNextLine();
            }
        }
    }

    // --- UPDATED: Added the Action parameter ---
    public void StartDialogue(DialogueLine[] linesToPlay, Action onComplete = null)
    {
        onDialogueComplete = onComplete; // Save the action for later
        
        Time.timeScale = 0f; 
        dialogueBox.SetActive(true);
        linesQueue.Clear();

        foreach (DialogueLine line in linesToPlay)
        {
            linesQueue.Enqueue(line);
        }

        DisplayNextLine();
    }

    public void DisplayNextLine()
    {
        if (linesQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        currentLine = linesQueue.Dequeue();
        
        if (currentLine.portrait != null)
        {
            portraitImage.sprite = currentLine.portrait;
            portraitImage.gameObject.SetActive(true);
        }
        else
        {
            portraitImage.gameObject.SetActive(false); 
        }

        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentLine.text));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed); 
        }
        
        isTyping = false;
    }

    void EndDialogue()
    {
        Debug.Log("Dialogue ended. Running callback if it exists.");
        dialogueBox.SetActive(false);
        Time.timeScale = 1f; 



        // --- NEW: Run the callback if one was provided ---
        onDialogueComplete?.Invoke();
        onDialogueComplete = null; // Clear it out so it doesn't accidentally fire again later
        Debug.Log(Time.timeScale);
    }
}